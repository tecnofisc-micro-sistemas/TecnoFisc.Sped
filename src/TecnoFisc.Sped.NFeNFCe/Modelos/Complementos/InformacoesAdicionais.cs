// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record InformacoesAdicionais
{
    /// <summary>XML: <c>infAdFisco</c>.</summary>
    public string? InfAdFisco { get; init; }

    /// <summary>XML: <c>infCpl</c>.</summary>
    public string? InfCpl { get; init; }

    /// <summary>XML: <c>obsCont</c>.</summary>
    public IReadOnlyList<InformacoesAdicionaisObsCont> ObsCont { get; init; } = [];

    /// <summary>XML: <c>obsFisco</c>.</summary>
    public IReadOnlyList<InformacoesAdicionaisObsFisco> ObsFisco { get; init; } = [];

    /// <summary>XML: <c>procRef</c>.</summary>
    public IReadOnlyList<InformacoesAdicionaisProcRef> ProcRef { get; init; } = [];
}
