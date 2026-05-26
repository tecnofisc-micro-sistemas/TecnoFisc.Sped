namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Opções de leitura do <see cref="ParserNFe"/>. Imutável (init-only) e reaproveitável entre
/// chamadas — o parser é stateless. Use <see cref="Default"/> para o comportamento padrão.
/// </summary>
public sealed class ParserNFeOptions
{
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
