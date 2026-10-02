namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Opções de leitura do <see cref="ParserNFe"/>. Imutável (init-only) e reaproveitável entre
/// chamadas — o parser é stateless. Use <see cref="Default"/> para o comportamento padrão.
/// </summary>
public sealed class ParserNFeOptions
{
    public int BufferCapacity { get; init; } = 64;
    public long MaxXmlBytes { get; init; } = 64L * 1024 * 1024;
    public int MaxXmlDepth { get; init; } = 128;
    public int MaxZipEntries { get; init; } = 100_000;
    public long MaxZipEntryBytes { get; init; } = 64L * 1024 * 1024;
    public long MaxZipUncompressedBytes { get; init; } = 100L * 1024 * 1024 * 1024;
    public long MaxZipSpoolBytes { get; init; } = 20L * 1024 * 1024 * 1024;
    public long MaxZipCentralDirectoryBytes { get; init; } = 64L * 1024 * 1024;
    public string? TemporaryDirectory { get; init; }
    /// <summary>Recebe metadados de campos ignorados; pode ser chamado concorrentemente. Exceções propagam.</summary>
    public Action<CampoXmlIgnorado>? OnIgnoredElement { get; init; }

    internal void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(BufferCapacity, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxXmlBytes, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxXmlDepth, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxZipEntries, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxZipEntryBytes, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxZipUncompressedBytes, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxZipSpoolBytes, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxZipCentralDirectoryBytes, 1);
    }

    private readonly int _parallelism = Environment.ProcessorCount;

    /// <summary>Instância padrão (todos os defaults).</summary>
    public static ParserNFeOptions Default { get; } = new();

    /// <summary>
    /// Quando <c>false</c> (padrão), elementos desconhecidos são ignorados em vez de lançar —
    /// protege contra Notas Técnicas futuras que acrescentem campos. Quando <c>true</c>, um
    /// elemento não mapeado interrompe a leitura.
    /// </summary>
    public bool Strict { get; init; }

    /// <summary>
    /// Quando <c>true</c> (padrão), valida o dígito verificador da chave de acesso e demais
    /// value objects (GTIN, etc.) na construção do modelo.
    /// </summary>
    public bool ValidateChecksums { get; init; } = true;

    /// <summary>
    /// Grau de paralelismo de <c>ReadDirectoryAsync</c> (caminho "milhões de XMLs"). Padrão =
    /// <see cref="Environment.ProcessorCount"/>. Deve ser maior ou igual a 1.
    /// </summary>
    public int Parallelism
    {
        get => _parallelism;
        init
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            _parallelism = value;
        }
    }

    /// <summary>
    /// Quando <c>true</c> (padrão), a primeira falha de parse interrompe a leitura. Quando
    /// <c>false</c>, em <c>ReadDirectoryAsync</c> o arquivo com erro é logado e pulado.
    /// </summary>
    public bool FailFast { get; init; } = true;
}
