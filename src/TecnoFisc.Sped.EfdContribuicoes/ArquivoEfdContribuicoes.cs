using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.EfdContribuicoes;

/// <summary>
/// Modelo raiz do arquivo EFD Contribuições (V006, guia v1.35). Agrupa os registros nos dez
/// blocos do leiaute na ordem canônica (0, A, C, D, F, I, M, P, 1, 9). Atende o contrato de
/// <see cref="IArquivoSped"/> e expõe acesso direto a cada bloco para consumidores tipados.
/// </summary>
/// <remarks>
/// Os registros já vêm vinculados via <c>PilhaHierarquica</c> quando o arquivo é montado a
/// partir do <see cref="Parser.ParserEfdContribuicoes"/>; o arquivo apenas os redistribui em
/// blocos preservando a ordem original. Para gravar, percorra <see cref="EnumerarRegistros"/>
/// e entregue ao <see cref="Gerador.GeradorEfdContribuicoes"/>.
/// </remarks>
public sealed class ArquivoEfdContribuicoes : IArquivoSped
{
    private static readonly string[] _ordemBlocos =
        ["0", "A", "C", "D", "F", "I", "M", "P", "1", "9"];

    private readonly Dictionary<string, BlocoEfdContribuicoes> _blocos;
    private readonly List<RegistroNaoReconhecido> _naoReconhecidos = [];

    public ArquivoEfdContribuicoes()
    {
        _blocos = new Dictionary<string, BlocoEfdContribuicoes>(_ordemBlocos.Length, StringComparer.Ordinal);
        foreach (var id in _ordemBlocos)
            _blocos.Add(id, new BlocoEfdContribuicoes(id));
    }

    public BlocoEfdContribuicoes Bloco0 => _blocos["0"];
    public BlocoEfdContribuicoes BlocoA => _blocos["A"];
    public BlocoEfdContribuicoes BlocoC => _blocos["C"];
    public BlocoEfdContribuicoes BlocoD => _blocos["D"];
    public BlocoEfdContribuicoes BlocoF => _blocos["F"];
    public BlocoEfdContribuicoes BlocoI => _blocos["I"];
    public BlocoEfdContribuicoes BlocoM => _blocos["M"];
    public BlocoEfdContribuicoes BlocoP => _blocos["P"];
    public BlocoEfdContribuicoes Bloco1 => _blocos["1"];
    public BlocoEfdContribuicoes Bloco9 => _blocos["9"];

    /// <summary>
    /// Registros que o leitor não conseguiu classificar — código desconhecido pelo catálogo ou
    /// descartado por vigência. Só é populado sob <c>LenientLayout</c> ou vigência ligada; sob
    /// leitura estrita o parser já teria abortado antes.
    /// Use <see cref="RegistroNaoReconhecido.Motivo"/> para separar as duas origens — a mensagem
    /// de <see cref="RegistroNaoReconhecido.Erro"/> é texto livre e não é contrato.
    /// </summary>
    public IReadOnlyList<RegistroNaoReconhecido> RegistrosNaoReconhecidos => _naoReconhecidos;

    /// <inheritdoc />
    public IEnumerable<IBlocoSped> EnumerarBlocos()
    {
        foreach (var id in _ordemBlocos)
            yield return _blocos[id];
    }

    /// <summary>Enumera todos os registros do arquivo na ordem canônica de gravação.</summary>
    public IEnumerable<RegistroSped> EnumerarRegistros()
    {
        foreach (var id in _ordemBlocos)
            foreach (var registro in _blocos[id].EnumerarRegistros())
                yield return registro;
    }

    /// <summary>
    /// Adiciona um registro ao bloco correspondente conforme a primeira posição do código
    /// (convenção do leiaute: o caractere inicial identifica o bloco).
    /// <see cref="RegistroNaoReconhecido"/> desvia para <see cref="RegistrosNaoReconhecidos"/>
    /// em vez de ser roteado por código — nunca lança. Qualquer outro registro cujo bloco não
    /// exista lança <see cref="InvalidOperationException"/>: é erro de uso da API (registro
    /// tipado de um bloco que a EFD Contribuições não tem), não dado ruim de arquivo.
    /// </summary>
    public void Adicionar(RegistroSped registro)
    {
        ArgumentNullException.ThrowIfNull(registro);

        if (registro is RegistroNaoReconhecido naoReconhecido)
        {
            _naoReconhecidos.Add(naoReconhecido);
            return;
        }

        var codigo = registro.Codigo;
        if (string.IsNullOrEmpty(codigo))
            throw new ArgumentException("Registro com código vazio não pode ser adicionado.", nameof(registro));

        var idBloco = char.ToUpperInvariant(codigo[0]).ToString();
        if (!_blocos.TryGetValue(idBloco, out var bloco))
            throw new InvalidOperationException(
                $"Código '{codigo}' não pertence a um bloco conhecido do leiaute EFD Contribuições.");

        bloco.Adicionar(registro);
    }

    /// <summary>
    /// Constrói o arquivo a partir do fluxo do parser, preservando a ordem dos registros
    /// dentro de cada bloco. Útil como ponte direta entre <see cref="Parser.ParserEfdContribuicoes"/>
    /// e a API tipada.
    /// </summary>
    public static async Task<ArquivoEfdContribuicoes> LoadAsync(
        IAsyncEnumerable<RegistroSped> registros,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(registros);

        var arquivo = new ArquivoEfdContribuicoes();
        await foreach (var registro in registros.WithCancellation(cancelamento).ConfigureAwait(false))
            arquivo.Adicionar(registro);
        return arquivo;
    }
}
