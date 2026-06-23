using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Core.Erros;

namespace TecnoFisc.Sped.Txt.Engine.Parser;

/// <summary>
/// Leitor SPED .txt baseado em <see cref="PipeReader"/>. Trabalha sobre o stream cru
/// (Latin1/Win-1252), recorta cada linha pelo <c>LF</c>, decodifica para
/// <see cref="ReadOnlySpan{Char}"/> usando buffers do <see cref="ArrayPool{T}"/> e
/// constrói o <see cref="RegistroSped"/> via catálogo. A vinculação Pai/Filhos é feita
/// pela <see cref="PilhaHierarquica"/>.
/// </summary>
/// <remarks>
/// Stage 2: o caminho de set de propriedade aloca uma string por campo (fallback). O
/// Stage 6 (source generator) elimina essa alocação substituindo o catálogo reflexivo
/// por código gerado em tempo de compilação. A API pública não muda.
/// <para>
/// Registros com campo-arquivo embutido (<see cref="MetadadosRegistro.EhMultilinha"/> —
/// ex.: J800/J801 da ECD) ocupam várias linhas físicas, pois o conteúdo do arquivo (RTF até
/// 30 MB) carrega quebras CRLF. Para esses, o leitor acumula as linhas físicas a partir do
/// início do registro até a que termina em <c>|{token}|</c> e só então interpreta o registro
/// montado, preservando as quebras internas.
/// </para>
/// </remarks>
public sealed class LeitorSpedTxt : ILeitorSped
{
    /// <summary>
    /// Código do registro de encerramento total do arquivo digital (Bloco 9, último registro
    /// do leiaute). Ao encontrá-lo, o leitor encerra o consumo do stream — qualquer conteúdo
    /// posterior (tipicamente o bloco de assinatura digital PKCS#7 anexado pelo PVA da Receita)
    /// é ignorado por não fazer parte do leiaute textual de registros. Convenção universal
    /// dos leiautes SPED .txt; se algum leiaute futuro divergir, promover para o catálogo.
    /// </summary>
    private const string CodigoEncerramentoArquivo = "9999";

    private readonly IRegistroSpedCatalogo _catalogo;
    private readonly ReadingOptions _opcoes;

    public LeitorSpedTxt(IRegistroSpedCatalogo catalogo)
        : this(catalogo, ReadingOptions.Default)
    {
    }

    public LeitorSpedTxt(IRegistroSpedCatalogo catalogo, ReadingOptions opcoes)
    {
        ArgumentNullException.ThrowIfNull(catalogo);
        ArgumentNullException.ThrowIfNull(opcoes);
        _catalogo = catalogo;
        _opcoes = opcoes;
    }

    public async IAsyncEnumerable<RegistroSped> ReadStreamingAsync(
        Stream entrada,
        [EnumeratorCancellation] CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);

        var leitor = PipeReader.Create(entrada);
        var pilha = new PilhaHierarquica();
        long numeroLinha = 0;
        bool encerrado = false;
        int versaoLeiaute = 0;

        // Estado de descarte (ReadingOptions). nivelCorteSubarvore >= 0 indica que estamos dentro
        // da subárvore de um registro ignorado por código; sobrevive entre iterações de ReadAsync.
        bool hasFilter = _opcoes.HasFilter;
        int nivelCorteSubarvore = -1;

        try
        {
            while (!encerrado)
            {
                cancelamento.ThrowIfCancellationRequested();

                var resultado = await leitor.ReadAsync(cancelamento).ConfigureAwait(false);
                var buffer = resultado.Buffer;

                while (TentarExtrairRegistro(ref buffer, resultado.IsCompleted,
                                             out var registroBytes, out var metadados, out int linhasFisicas))
                {
                    long linhaRegistro = numeroLinha + 1;
                    numeroLinha += linhasFisicas;

                    // Descarte antes de materializar: registro ignorado não é decodificado (multi-linha
                    // não paga o custo dos 30 MB do ARQ_RTF) nem entra na hierarquia/stream.
                    if (hasFilter && ShouldIgnore(metadados, ref nivelCorteSubarvore))
                    {
                        if (metadados is not null && metadados.Codigo == CodigoEncerramentoArquivo)
                        {
                            // Mesmo ignorado, o 9999 encerra o consumo (evita ler a assinatura anexa).
                            encerrado = true;
                            break;
                        }
                        continue;
                    }

                    var registro = ProcessarLinha(in registroBytes, linhaRegistro, pilha, versaoLeiaute, metadados);
                    if (registro is not null)
                    {
                        // Captura a versão do leiaute assim que o Registro0000 é processado.
                        if (versaoLeiaute == 0 && registro.VersaoLeiaute > 0)
                            versaoLeiaute = registro.VersaoLeiaute;

                        yield return registro;
                        if (registro.Codigo == CodigoEncerramentoArquivo)
                        {
                            encerrado = true;
                            break;
                        }
                    }
                }

                if (encerrado)
                {
                    leitor.AdvanceTo(buffer.Start, buffer.Start);
                    break;
                }

                leitor.AdvanceTo(buffer.Start, buffer.End);

                if (resultado.IsCompleted)
                    break;
            }
        }
        finally
        {
            await leitor.CompleteAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Parseia uma única linha SPED canônica (<c>|REG|...|</c>) isoladamente, sem hierarquia nem
    /// streaming. Nunca lança por erro de campo: o registro (em <see cref="ResultadoParse{T}.Valor"/>)
    /// traz os campos conversíveis preenchidos e os que falharam no valor default, com os erros em
    /// <see cref="Abstracoes.RegistroSped.ErrosDeFormato"/>. Devolve falha apenas quando nenhum
    /// registro pôde ser produzido (linha sem '|' nas pontas ou código desconhecido pelo catálogo).
    /// </summary>
    public ResultadoParse<RegistroSped> ParseLinha(ReadOnlySpan<char> linha, long numeroLinha = 0)
    {
        if (linha.IsEmpty || linha[0] != '|' || linha[^1] != '|')
            return ResultadoParse<RegistroSped>.Falhar(
                new ErroFormato(numeroLinha, null, null, "Linha SPED deve iniciar e terminar com '|'.")
                {
                    ValorBruto = linha.IsEmpty ? null : linha.ToString()
                });

        var pilha = new PilhaHierarquica();   // descartável: ParseLinha não constrói hierarquia
        var registro = InterpretarLinha(linha, numeroLinha, pilha, versaoLeiaute: 0,
            metadadosResolvido: null, forcarLenienteCampo: true, forcarLenienteLayout: true);

        if (registro is RegistroNaoReconhecido sentinela)
            return ResultadoParse<RegistroSped>.Falhar(
                new ErroFormato(numeroLinha, sentinela.Codigo, null, sentinela.Erro.Mensagem));

        return registro is null
            ? ResultadoParse<RegistroSped>.Falhar(
                new ErroFormato(numeroLinha, null, null, "Linha não produziu registro."))
            : ResultadoParse<RegistroSped>.Ok(registro);
    }

    /// <summary>
    /// Decide se o registro deve ser descartado conforme <see cref="ReadingOptions"/>:
    /// <list type="bullet">
    ///   <item>se estamos na subárvore de um registro ignorado por código, descarta descendentes
    ///   (nível maior) até sair (nível menor ou igual ao do corte);</item>
    ///   <item><see cref="ReadingOptions.BlocosIgnorados"/>: descarta qualquer registro do bloco
    ///   (todos carregam o mesmo bloco — não precisa de subárvore);</item>
    ///   <item><see cref="ReadingOptions.RegistrosIgnorados"/>: descarta o registro e abre a
    ///   subárvore para descartar os filhos.</item>
    /// </list>
    /// Metadados nulo (linha vazia ou código desconhecido) nunca é ignorado — segue o caminho
    /// normal (que produz <c>null</c> ou erro de layout).
    /// </summary>
    private bool ShouldIgnore(MetadadosRegistro? metadados, ref int nivelCorteSubarvore)
    {
        if (metadados is null)
            return false;

        if (nivelCorteSubarvore >= 0)
        {
            if (metadados.Nivel > nivelCorteSubarvore)
                return true;
            nivelCorteSubarvore = -1; // nível menor ou igual: saiu da subárvore ignorada.
        }

        if (_opcoes.BlocosIgnorados.Contains(metadados.Bloco))
            return true;

        if (_opcoes.RegistrosIgnorados.Contains(metadados.Codigo))
        {
            nivelCorteSubarvore = metadados.Nivel;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Extrai o próximo registro lógico do <paramref name="buffer"/>. Para registros de uma linha
    /// física, recorta pelo <c>LF</c>. Para registros multi-linha (campo-arquivo embutido),
    /// acumula linhas físicas até a que termina em <c>|{token}|</c>. Avança o <paramref name="buffer"/>
    /// somente quando devolve um registro completo; caso contrário (precisa de mais dados) deixa
    /// o <paramref name="buffer"/> intacto para o <see cref="PipeReader"/> bufferizar mais.
    /// </summary>
    private bool TentarExtrairRegistro(
        ref ReadOnlySequence<byte> buffer,
        bool entradaCompleta,
        out ReadOnlySequence<byte> registro,
        out MetadadosRegistro? metadados,
        out int linhasFisicas)
    {
        registro = default;
        metadados = null;
        linhasFisicas = 0;

        if (buffer.IsEmpty)
            return false;

        var posLf = buffer.PositionOf(EncodingSped.LfAscii);
        if (posLf is null && !entradaCompleta)
            return false; // linha incompleta; aguarda mais dados.

        var primeiraLinha = posLf is null ? buffer : buffer.Slice(0, posLf.Value);
        metadados = ResolverMetadados(primeiraLinha);

        if (metadados is { EhMultilinha: true })
            return TentarExtrairMultilinha(ref buffer, metadados, entradaCompleta, out registro, out linhasFisicas);

        // Registro de uma linha física (caminho clássico).
        linhasFisicas = 1;
        if (posLf is null)
        {
            // entradaCompleta garantido aqui: última linha sem terminador LF.
            registro = AparaCrFinal(buffer);
            buffer = buffer.Slice(buffer.End);
            return true;
        }

        registro = AparaCrFinal(primeiraLinha);
        buffer = buffer.Slice(buffer.GetPosition(1, posLf.Value));
        return true;
    }

    /// <summary>
    /// Acumula as linhas físicas de um registro multi-linha até a linha que termina em
    /// <c>|{token}|</c> (<see cref="MetadadosRegistro.TokenFimArquivo"/>). O registro devolvido
    /// inclui as quebras CRLF internas do campo-arquivo. Devolve <c>false</c> sem mexer no
    /// <paramref name="buffer"/> quando o terminador ainda não está no buffer e a entrada não
    /// terminou; lança <see cref="ErroFormatoSpedException"/> se a entrada acabou sem o terminador.
    /// </summary>
    private static bool TentarExtrairMultilinha(
        ref ReadOnlySequence<byte> buffer,
        MetadadosRegistro metadados,
        bool entradaCompleta,
        out ReadOnlySequence<byte> registro,
        out int linhasFisicas)
    {
        registro = default;
        linhasFisicas = 0;

        string token = metadados.TokenFimArquivo!;
        // needle = |{token}| — ex.: |J800FIM|
        Span<byte> needle = stackalloc byte[token.Length + 2];
        needle[0] = EncodingSped.PipeAscii;
        EncodingSped.Latin1.GetBytes(token, needle[1..^1]);
        needle[^1] = EncodingSped.PipeAscii;

        var reader = new SequenceReader<byte>(buffer);
        while (reader.TryReadTo(out ReadOnlySequence<byte> _, needle, advancePastDelimiter: true))
        {
            var fimRegistro = reader.Position; // logo após o '|' de fechamento do needle.

            // Confirma que o needle está em fim de linha física (próximo byte é CR/LF ou EOF);
            // do contrário é um falso positivo (token no meio do conteúdo) — continua buscando.
            if (TentarConsumirQuebra(buffer, fimRegistro, entradaCompleta, out var aposQuebra, out bool precisaMais))
            {
                registro = buffer.Slice(0, fimRegistro);
                linhasFisicas = ContarLinhas(buffer.Slice(0, aposQuebra));
                buffer = buffer.Slice(aposQuebra);
                return true;
            }

            if (precisaMais)
                return false; // needle no fim do buffer; precisa de mais bytes p/ confirmar a quebra.
        }

        if (!entradaCompleta)
            return false; // terminador ainda não chegou; aguarda mais dados.

        throw new ErroFormatoSpedException(new ErroFormato(
            0, metadados.Codigo, null,
            $"Registro multi-linha '{metadados.Codigo}' sem o terminador '{token}' (arquivo truncado)."));
    }

    /// <summary>
    /// Verifica se a posição <paramref name="fim"/> está no fim de uma linha física e devolve a
    /// posição logo após a quebra (CR, LF ou CRLF). Define <paramref name="precisaMais"/> quando
    /// o byte de quebra ainda não chegou ao buffer e a entrada não terminou.
    /// </summary>
    private static bool TentarConsumirQuebra(
        ReadOnlySequence<byte> buffer,
        SequencePosition fim,
        bool entradaCompleta,
        out SequencePosition aposQuebra,
        out bool precisaMais)
    {
        aposQuebra = fim;
        precisaMais = false;

        var resto = buffer.Slice(fim);
        if (resto.IsEmpty)
        {
            if (entradaCompleta)
                return true; // último registro sem terminador LF final.
            precisaMais = true;
            return false;
        }

        var r = new SequenceReader<byte>(resto);
        r.TryPeek(out byte b0);

        if (b0 == EncodingSped.LfAscii)
        {
            aposQuebra = resto.GetPosition(1);
            return true;
        }

        if (b0 == EncodingSped.CrAscii)
        {
            if (resto.Length >= 2)
            {
                r.TryPeek(1, out byte b1);
                aposQuebra = resto.GetPosition(b1 == EncodingSped.LfAscii ? 2 : 1);
                return true;
            }
            if (entradaCompleta)
            {
                aposQuebra = resto.GetPosition(1);
                return true;
            }
            precisaMais = true;
            return false;
        }

        // Outro byte após o needle → não é fim de linha física (falso positivo).
        return false;
    }

    /// <summary>Conta as linhas físicas em uma região (número de <c>LF</c> + a última sem LF).</summary>
    private static int ContarLinhas(ReadOnlySequence<byte> regiao)
    {
        if (regiao.IsEmpty)
            return 1;

        long lfs = 0;
        var r = new SequenceReader<byte>(regiao);
        while (r.TryAdvanceTo(EncodingSped.LfAscii, advancePastDelimiter: true))
            lfs++;

        int linhas = (int)lfs;
        if (UltimoByte(in regiao) != EncodingSped.LfAscii)
            linhas++; // última linha física sem terminador LF.
        return Math.Max(linhas, 1);
    }

    /// <summary>
    /// Resolve os metadados do registro a partir do código (entre o 1º e o 2º <c>|</c>) sem
    /// alocar string. Devolve <c>null</c> para linhas vazias, malformadas ou código desconhecido —
    /// o caminho de uma linha trata esses casos em <see cref="InterpretarLinha"/>.
    /// </summary>
    private MetadadosRegistro? ResolverMetadados(ReadOnlySequence<byte> linha)
    {
        var primeiroPipe = linha.PositionOf(EncodingSped.PipeAscii);
        if (primeiroPipe is null)
            return null;

        var aposPrimeiro = linha.Slice(linha.GetPosition(1, primeiroPipe.Value));
        var segundoPipe = aposPrimeiro.PositionOf(EncodingSped.PipeAscii);
        if (segundoPipe is null)
            return null;

        var codigoBytes = aposPrimeiro.Slice(0, segundoPipe.Value);
        int comprimento = (int)codigoBytes.Length;
        if (comprimento is 0 or > 32)
            return null;

        Span<byte> bytes = stackalloc byte[32];
        codigoBytes.CopyTo(bytes);
        Span<char> chars = stackalloc char[32];
        int qtd = EncodingSped.Latin1.GetChars(bytes[..comprimento], chars);

        return _catalogo.TentarObter(chars[..qtd], out var metadados) ? metadados : null;
    }

    private static ReadOnlySequence<byte> AparaCrFinal(in ReadOnlySequence<byte> linha)
    {
        if (!linha.IsEmpty && UltimoByte(in linha) == EncodingSped.CrAscii)
            return linha.Slice(0, linha.Length - 1);
        return linha;
    }

    private static byte UltimoByte(in ReadOnlySequence<byte> sequencia)
    {
        if (sequencia.IsEmpty)
            return 0;

        if (sequencia.IsSingleSegment)
            return sequencia.FirstSpan[^1];

        long alvo = sequencia.Length - 1;
        long acumulado = 0;
        foreach (var memoria in sequencia)
        {
            if (acumulado + memoria.Length > alvo)
                return memoria.Span[(int)(alvo - acumulado)];
            acumulado += memoria.Length;
        }
        return 0;
    }

    private RegistroSped? ProcessarLinha(
        in ReadOnlySequence<byte> linha,
        long numeroLinha,
        PilhaHierarquica pilha,
        int versaoLeiaute,
        MetadadosRegistro? metadadosResolvido)
    {
        int comprimento = checked((int)linha.Length);
        if (comprimento == 0)
            return null;

        var bytesAlugados = ArrayPool<byte>.Shared.Rent(comprimento);
        char[]? charsAlugados = null;
        try
        {
            var bytes = bytesAlugados.AsSpan(0, comprimento);
            linha.CopyTo(bytes);

            int qtdChar = EncodingSped.Latin1.GetCharCount(bytes);
            charsAlugados = ArrayPool<char>.Shared.Rent(qtdChar);
            var chars = charsAlugados.AsSpan(0, qtdChar);
            int gravados = EncodingSped.Latin1.GetChars(bytes, chars);

            return InterpretarLinha(chars[..gravados], numeroLinha, pilha, versaoLeiaute, metadadosResolvido);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytesAlugados);
            if (charsAlugados is not null)
                ArrayPool<char>.Shared.Return(charsAlugados);
        }
    }

    private RegistroSped? InterpretarLinha(
        ReadOnlySpan<char> linha,
        long numeroLinha,
        PilhaHierarquica pilha,
        int versaoLeiaute,
        MetadadosRegistro? metadadosResolvido,
        bool? forcarLenienteCampo = null,
        bool? forcarLenienteLayout = null)
    {
        if (linha.IsEmpty)
            return null;

        if (linha[0] != '|')
            throw new ErroFormatoSpedException(
                new ErroFormato(numeroLinha, null, null, "Linha SPED deve iniciar com '|'."));

        if (linha[^1] != '|')
            throw new ErroFormatoSpedException(
                new ErroFormato(numeroLinha, null, null, "Linha SPED deve terminar com '|'."));

        // remove pipes inicial e final; o conteúdo restante é separado por '|'.
        var conteudo = linha[1..^1];

        bool lenienteCampo = forcarLenienteCampo ?? _opcoes.LenientFieldParsing;
        bool lenienteLayout = forcarLenienteLayout ?? _opcoes.LenientLayout;

        MetadadosRegistro? metadados = null;
        RegistroSped? registro = null;

        // Aplica um campo; em modo leniente, captura a falha de conversão, acumula no registro
        // (campo permanece no default) e segue. Em modo estrito, mantém o comportamento atual.
        void Definir(MetadadosCampo campo, ReadOnlySpan<char> valor)
        {
            try
            {
                campo.Definidor(registro!, valor);
            }
            catch (Exception ex) when (ex is FormatException or ArgumentException or OverflowException)
            {
                var erro = new ErroFormato(numeroLinha, metadados!.Codigo, campo.Nome, ex.Message)
                {
                    ValorBruto = valor.ToString()
                };
                if (!lenienteCampo)
                    throw new ErroFormatoSpedException(erro, ex);
                registro!.RegistrarErroDeFormato(erro);
            }
        }
        // Posição na nomenclatura do Guia Prático: 1 = REG; 2..N = campos do layout.
        int posicaoCampo = 1;
        int inicioCampo = 0;

        for (int i = 0; i <= conteudo.Length; i++)
        {
            if (i != conteudo.Length && conteudo[i] != '|')
                continue;

            var fatia = conteudo[inicioCampo..i];

            if (posicaoCampo == 1)
            {
                metadados = metadadosResolvido;
                if (metadados is null && !_catalogo.TentarObter(fatia, out metadados))
                {
                    var erroLayout = new ErroLayout(numeroLinha, fatia.ToString(),
                        "Código de registro desconhecido pelo catálogo.");
                    if (!lenienteLayout)
                        throw new ErroLayoutSpedException(erroLayout);

                    // Sentinela: pendura como folha no topo atual (sem empilhar, nunca vira pai).
                    var sentinela = new RegistroNaoReconhecido(fatia.ToString(), linha.ToString(), erroLayout);
                    pilha.Topo?.AdicionarFilho(sentinela);
                    return sentinela;
                }

                // [Descontinuado] é informacional no read path (ARCHITECTURE §4.7 read-only):
                // arquivos históricos ainda contêm o registro e precisam ser parseáveis.
                registro = metadados.Fabrica();
            }
            else if (metadados is not null && registro is not null)
            {
                int indice = posicaoCampo - 2;
                if (indice < metadados.Campos.Count)
                {
                    var campo = metadados.Campos[indice];
                    if (campo.CapturaTudo)
                    {
                        // Campo variádico (*): captura tudo que resta na linha a partir
                        // de inicioCampo, incluindo os separadores | intermediários.
                        Definir(campo, conteudo[inicioCampo..]);
                        break;
                    }
                    if (campo.CampoArquivo)
                    {
                        // Campo-arquivo de registro multi-linha (ex.: ARQ_RTF): captura tudo
                        // entre inicioCampo e o último '|' do conteúdo (que separa do token de
                        // fim), preservando '|' e CRLFs embutidos. O campo seguinte é o token.
                        var resto = conteudo[inicioCampo..];
                        int idxSep = resto.LastIndexOf('|');
                        if (idxSep < 0)
                        {
                            Definir(campo, resto);
                            break;
                        }
                        Definir(campo, resto[..idxSep]);
                        if (indice + 1 < metadados.Campos.Count)
                            Definir(metadados.Campos[indice + 1], resto[(idxSep + 1)..]);
                        break;
                    }
                    Definir(campo, fatia);
                }
                // Campos posteriores ao último declarado são ignorados — layouts novos
                // podem adicionar colunas no fim sem quebrar leitores antigos.
            }

            posicaoCampo++;
            inicioCampo = i + 1;
        }

        if (registro is null || metadados is null)
            return null;

        var pai = pilha.Empilhar(registro, metadados.Nivel);
        pai?.AdicionarFilho(registro);
        return registro;
    }
}
