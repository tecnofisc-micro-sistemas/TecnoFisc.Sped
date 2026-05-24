using TecnoFisc.Sped.Core.Abstracoes;

namespace TecnoFisc.Sped.EfdIcmsIpi;

/// <summary>
/// Modelo raiz do arquivo EFD ICMS-IPI (baseline V015). Agrupa os registros nos dez
/// blocos do leiaute na ordem canônica (0, B, C, D, E, G, H, K, 1, 9).
/// </summary>
/// <remarks>
/// Pacote read-only (ARCHITECTURE §2.5): os registros vêm vinculados via
/// <c>PilhaHierarquica</c> a partir do <see cref="Parser.ParserEfdIcmsIpi"/>; o arquivo apenas
/// os redistribui em blocos preservando a ordem original. Não existe gerador associado —
/// EFD ICMS-IPI é exclusivamente leitura.
/// </remarks>
public sealed class ArquivoEfdIcmsIpi : IArquivoSped
{
    private static readonly string[] _ordemBlocos =
        ["0", "B", "C", "D", "E", "G", "H", "K", "1", "9"];

    private readonly Dictionary<string, BlocoEfdIcmsIpi> _blocos;

    public ArquivoEfdIcmsIpi()
    {
        _blocos = new Dictionary<string, BlocoEfdIcmsIpi>(_ordemBlocos.Length, StringComparer.Ordinal);
        foreach (var id in _ordemBlocos)
            _blocos.Add(id, new BlocoEfdIcmsIpi(id));
    }

    public BlocoEfdIcmsIpi Bloco0 => _blocos["0"];
    public BlocoEfdIcmsIpi BlocoB => _blocos["B"];
    public BlocoEfdIcmsIpi BlocoC => _blocos["C"];
    public BlocoEfdIcmsIpi BlocoD => _blocos["D"];
    public BlocoEfdIcmsIpi BlocoE => _blocos["E"];
    public BlocoEfdIcmsIpi BlocoG => _blocos["G"];
    public BlocoEfdIcmsIpi BlocoH => _blocos["H"];
    public BlocoEfdIcmsIpi BlocoK => _blocos["K"];
    public BlocoEfdIcmsIpi Bloco1 => _blocos["1"];
    public BlocoEfdIcmsIpi Bloco9 => _blocos["9"];

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
    /// Códigos fora dos dez blocos conhecidos lançam exceção.
    /// </summary>
    public void Adicionar(RegistroSped registro)
    {
        ArgumentNullException.ThrowIfNull(registro);

        var codigo = registro.Codigo;
        if (string.IsNullOrEmpty(codigo))
            throw new ArgumentException("Registro com código vazio não pode ser adicionado.", nameof(registro));

        var idBloco = char.ToUpperInvariant(codigo[0]).ToString();
        if (!_blocos.TryGetValue(idBloco, out var bloco))
            throw new InvalidOperationException(
                $"Código '{codigo}' não pertence a um bloco conhecido do leiaute EFD ICMS-IPI.");

        bloco.Adicionar(registro);
    }

    /// <summary>
    /// Constrói o arquivo a partir do fluxo do parser, preservando a ordem dos registros
    /// dentro de cada bloco.
    /// </summary>
    public static async Task<ArquivoEfdIcmsIpi> LoadAsync(
        IAsyncEnumerable<RegistroSped> registros,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(registros);

        var arquivo = new ArquivoEfdIcmsIpi();
        await foreach (var registro in registros.WithCancellation(cancelamento).ConfigureAwait(false))
            arquivo.Adicionar(registro);
        return arquivo;
    }
}
