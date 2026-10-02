// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record IdentificacaoGPagAntecipado
{
    /// <summary>XML: <c>refNFe</c>.</summary>
    public IReadOnlyList<string> RefNFe { get; init; } = [];
}
