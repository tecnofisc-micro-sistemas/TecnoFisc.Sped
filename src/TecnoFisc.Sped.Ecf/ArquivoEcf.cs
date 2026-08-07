using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf;

/// <summary>
/// Modelo raiz read-only de um arquivo ECF. Agrupa os registros nos 17 blocos do leiaute
/// em sua ordem canônica.
/// </summary>
public sealed class ArquivoEcf : IArquivoSped
{
    private static readonly string[] _ordemBlocos = ["0", "C", "E", "J", "K", "L", "M", "N", "P", "Q", "T", "U", "V", "W", "X", "Y", "9"];

    private readonly Dictionary<string, BlocoEcf> _blocos;
    private readonly List<RegistroNaoReconhecido> _naoReconhecidos = [];

    public ArquivoEcf()
    {
        _blocos = new Dictionary<string, BlocoEcf>(_ordemBlocos.Length, StringComparer.Ordinal);
        foreach (var id in _ordemBlocos)
            _blocos.Add(id, new BlocoEcf(id));
    }

    public BlocoEcf Bloco0 => _blocos["0"];
    public BlocoEcf BlocoC => _blocos["C"];
    public BlocoEcf BlocoE => _blocos["E"];
    public BlocoEcf BlocoJ => _blocos["J"];
    public BlocoEcf BlocoK => _blocos["K"];
    public BlocoEcf BlocoL => _blocos["L"];
    public BlocoEcf BlocoM => _blocos["M"];
    public BlocoEcf BlocoN => _blocos["N"];
    public BlocoEcf BlocoP => _blocos["P"];
    public BlocoEcf BlocoQ => _blocos["Q"];
    public BlocoEcf BlocoT => _blocos["T"];
    public BlocoEcf BlocoU => _blocos["U"];
    public BlocoEcf BlocoV => _blocos["V"];
    public BlocoEcf BlocoW => _blocos["W"];
    public BlocoEcf BlocoX => _blocos["X"];
    public BlocoEcf BlocoY => _blocos["Y"];
    public BlocoEcf Bloco9 => _blocos["9"];

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

    /// <summary>Enumera todos os registros na ordem canônica dos blocos.</summary>
    public IEnumerable<RegistroSped> EnumerarRegistros()
    {
        foreach (var id in _ordemBlocos)
            foreach (var registro in _blocos[id].EnumerarRegistros())
                yield return registro;
    }

    /// <summary>Adiciona um registro ao bloco correspondente à primeira posição do código.</summary>
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
                $"Código '{codigo}' não pertence a um bloco conhecido do leiaute ECF.");

        bloco.Adicionar(registro);
    }

    /// <summary>Constrói o arquivo a partir do fluxo do parser.</summary>
    public static async Task<ArquivoEcf> LoadAsync(
        IAsyncEnumerable<RegistroSped> registros,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(registros);

        var arquivo = new ArquivoEcf();
        await foreach (var registro in registros.WithCancellation(cancelamento).ConfigureAwait(false))
            arquivo.Adicionar(registro);
        return arquivo;
    }
}
