namespace TecnoFisc.Sped.Txt.Engine.Abstracoes;

/// <summary>
/// Modelo raiz comum aos leiautes: agrupa registros em blocos na ordem canônica, roteia
/// <see cref="RegistroNaoReconhecido"/> para uma coleção à parte e enumera blocos e registros.
/// Cada leiaute concreto fornece sua ordem de blocos, sua fábrica de blocos e como adicionar a
/// um bloco — este último porque o <c>Adicionar</c> de cada bloco é <c>internal</c> ao assembly
/// do próprio leiaute, deliberadamente fora da API pública do modelo somente leitura.
/// </summary>
/// <typeparam name="TBloco">Tipo de bloco do leiaute.</typeparam>
public abstract class ArquivoSpedBase<TBloco> : IArquivoSped
    where TBloco : IBlocoSped
{
    private readonly string[] _ordemBlocos;
    private readonly Dictionary<string, TBloco> _blocos;
    private readonly List<RegistroNaoReconhecido> _naoReconhecidos = [];

    /// <param name="ordemBlocos">Identificadores dos blocos na ordem canônica do leiaute.</param>
    /// <param name="criarBloco">Fábrica do bloco, chamada uma vez por identificador.</param>
    protected ArquivoSpedBase(string[] ordemBlocos, Func<string, TBloco> criarBloco)
    {
        ArgumentNullException.ThrowIfNull(ordemBlocos);
        ArgumentNullException.ThrowIfNull(criarBloco);
        _ordemBlocos = ordemBlocos;
        _blocos = new Dictionary<string, TBloco>(ordemBlocos.Length, StringComparer.Ordinal);
        foreach (var id in ordemBlocos)
            _blocos.Add(id, criarBloco(id));
    }

    /// <summary>Nome do leiaute, usado na mensagem de erro de roteamento.</summary>
    protected abstract string NomeDoLeiaute { get; }

    /// <summary>
    /// Adiciona ao bloco. Implementado por leiaute porque o <c>Adicionar</c> de cada bloco é
    /// <c>internal</c> ao seu próprio assembly.
    /// </summary>
    protected abstract void AdicionarAoBloco(TBloco bloco, RegistroSped registro);

    /// <summary>Bloco pelo identificador. Lança se o bloco não existir no leiaute.</summary>
    protected TBloco Bloco(string id) => _blocos[id];

    /// <summary>
    /// Registros que o leitor não conseguiu classificar — código desconhecido pelo catálogo ou
    /// descartado por vigência. Só é populado sob <c>LenientLayout</c> ou vigência ligada; sob
    /// leitura estrita o parser já teria abortado antes. Use
    /// <see cref="RegistroNaoReconhecido.Motivo"/> para separar as duas origens — a mensagem de
    /// <see cref="RegistroNaoReconhecido.Erro"/> é texto livre e não é contrato.
    /// </summary>
    public IReadOnlyList<RegistroNaoReconhecido> RegistrosNaoReconhecidos => _naoReconhecidos;

    /// <inheritdoc />
    public IEnumerable<IBlocoSped> EnumerarBlocos()
    {
        foreach (var id in _ordemBlocos)
            yield return _blocos[id];
    }

    /// <summary>Enumera todos os registros na ordem canônica dos blocos.</summary>
    public IEnumerable<RegistroSped> EnumerarRegistros()
    {
        foreach (var id in _ordemBlocos)
            foreach (var registro in _blocos[id].EnumerarRegistros())
                yield return registro;
    }

    /// <summary>
    /// Adiciona um registro ao bloco correspondente à primeira posição do código.
    /// <see cref="RegistroNaoReconhecido"/> desvia para <see cref="RegistrosNaoReconhecidos"/> em
    /// vez de ser roteado por código — nunca lança. Qualquer outro registro cujo bloco não exista
    /// lança <see cref="InvalidOperationException"/>: é erro de uso da API (registro tipado de um
    /// bloco que o leiaute não tem), não dado ruim de arquivo.
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
                $"Código '{codigo}' não pertence a um bloco conhecido do leiaute {NomeDoLeiaute}.");

        AdicionarAoBloco(bloco, registro);
    }

    /// <summary>Consome o fluxo do parser preenchendo este arquivo.</summary>
    protected async Task PreencherAsync(
        IAsyncEnumerable<RegistroSped> registros, CancellationToken cancelamento)
    {
        ArgumentNullException.ThrowIfNull(registros);
        await foreach (var registro in registros.WithCancellation(cancelamento).ConfigureAwait(false))
            Adicionar(registro);
    }
}
