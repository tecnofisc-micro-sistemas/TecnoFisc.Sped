using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Erros;

namespace TecnoFisc.Sped.Core.Parser;

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

    public LeitorSpedTxt(IRegistroSpedCatalogo catalogo)
    {
        ArgumentNullException.ThrowIfNull(catalogo);
        _catalogo = catalogo;
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

        try
        {
            while (!encerrado)
            {
                cancelamento.ThrowIfCancellationRequested();

                var resultado = await leitor.ReadAsync(cancelamento).ConfigureAwait(false);
                var buffer = resultado.Buffer;

                while (TentarExtrairLinha(ref buffer, out var linha))
                {
                    numeroLinha++;
                    var registro = ProcessarLinha(in linha, numeroLinha, pilha, versaoLeiaute);
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
                {
                    if (!buffer.IsEmpty)
                    {
                        numeroLinha++;
                        var registro = ProcessarLinha(in buffer, numeroLinha, pilha, versaoLeiaute);
                        if (registro is not null)
                        {
                            if (versaoLeiaute == 0 && registro.VersaoLeiaute > 0)
                                versaoLeiaute = registro.VersaoLeiaute;
                            yield return registro;
                        }
                    }
                    break;
                }
            }
        }
        finally
        {
            await leitor.CompleteAsync().ConfigureAwait(false);
        }
    }

    private static bool TentarExtrairLinha(
        ref ReadOnlySequence<byte> buffer,
        out ReadOnlySequence<byte> linha)
    {
        var posLf = buffer.PositionOf(EncodingSped.LfAscii);
        if (posLf is null)
        {
            linha = default;
            return false;
        }

        linha = buffer.Slice(0, posLf.Value);
        var apos = buffer.GetPosition(1, posLf.Value);
        buffer = buffer.Slice(apos);

        if (!linha.IsEmpty && UltimoByte(in linha) == EncodingSped.CrAscii)
            linha = linha.Slice(0, linha.Length - 1);

        return true;
    }

    private static byte UltimoByte(in ReadOnlySequence<byte> sequencia)
    {
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
        int versaoLeiaute)
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

            return InterpretarLinha(chars[..gravados], numeroLinha, pilha, versaoLeiaute);
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
        int versaoLeiaute)
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

        MetadadosRegistro? metadados = null;
        RegistroSped? registro = null;
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
                if (!_catalogo.TentarObter(fatia, out metadados))
                    throw new ErroLayoutSpedException(
                        new ErroLayout(numeroLinha, fatia.ToString(),
                            "Código de registro desconhecido pelo catálogo."));

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
                    try
                    {
                        if (campo.CapturaTudo)
                        {
                            // Campo variádico (*): captura tudo que resta na linha a partir
                            // de inicioCampo, incluindo os separadores | intermediários.
                            campo.Definidor(registro, conteudo[inicioCampo..]);
                            break;
                        }
                        campo.Definidor(registro, fatia);
                    }
                    catch (Exception ex) when (ex is FormatException
                                                  or ArgumentException
                                                  or OverflowException)
                    {
                        throw new ErroFormatoSpedException(
                            new ErroFormato(numeroLinha, metadados.Codigo, campo.Nome, ex.Message),
                            ex);
                    }
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
