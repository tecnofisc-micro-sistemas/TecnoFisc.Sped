using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecd;

/// <summary>
/// Modelo raiz do arquivo ECD (Escrituração Contábil Digital — leiaute 9). Agrupa os
/// registros nos seis blocos do leiaute na ordem canônica (0, C, I, J, K, 9).
/// </summary>
/// <remarks>
/// Pacote read-only (ARCHITECTURE §2.5): os registros vêm vinculados via
/// <c>PilhaHierarquica</c> a partir do <see cref="Parser.ParserEcd"/>; o arquivo apenas os
/// redistribui em blocos preservando a ordem original. Não existe gerador associado — a ECD
/// é exclusivamente leitura.
/// </remarks>
public sealed class ArquivoEcd : IArquivoSped
{
    private static readonly string[] _ordemBlocos =
        ["0", "C", "I", "J", "K", "9"];

    private readonly Dictionary<string, BlocoEcd> _blocos;
    private readonly List<RegistroNaoReconhecido> _naoReconhecidos = [];

    public ArquivoEcd()
    {
        _blocos = new Dictionary<string, BlocoEcd>(_ordemBlocos.Length, StringComparer.Ordinal);
        foreach (var id in _ordemBlocos)
            _blocos.Add(id, new BlocoEcd(id));
    }

    public BlocoEcd Bloco0 => _blocos["0"];
    public BlocoEcd BlocoC => _blocos["C"];
    public BlocoEcd BlocoI => _blocos["I"];
    public BlocoEcd BlocoJ => _blocos["J"];
    public BlocoEcd BlocoK => _blocos["K"];
    public BlocoEcd Bloco9 => _blocos["9"];

    /// <summary>
    /// Registros que o leitor não conseguiu classificar — código desconhecido pelo catálogo ou
    /// descartado por vigência. Só é populado sob <c>LenientLayout</c> ou vigência ligada; sob
    /// leitura estrita o parser já teria abortado antes.
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
    /// Adiciona um registro ao bloco correspondente conforme a primeira posição do código.
    /// <see cref="RegistroNaoReconhecido"/> desvia para <see cref="RegistrosNaoReconhecidos"/>
    /// em vez de ser roteado por código — nunca lança. Qualquer outro registro cujo bloco não
    /// exista lança <see cref="InvalidOperationException"/>: é erro de uso da API (registro
    /// tipado de um bloco que a ECD não tem), não dado ruim de arquivo.
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
                $"Código '{codigo}' não pertence a um bloco conhecido do leiaute ECD.");

        bloco.Adicionar(registro);
    }

    /// <summary>
    /// Constrói o arquivo a partir do fluxo do parser, preservando a ordem dos registros
    /// dentro de cada bloco.
    /// </summary>
    public static async Task<ArquivoEcd> LoadAsync(
        IAsyncEnumerable<RegistroSped> registros,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(registros);

        var arquivo = new ArquivoEcd();
        await foreach (var registro in registros.WithCancellation(cancelamento).ConfigureAwait(false))
            arquivo.Adicionar(registro);
        return arquivo;
    }
}
