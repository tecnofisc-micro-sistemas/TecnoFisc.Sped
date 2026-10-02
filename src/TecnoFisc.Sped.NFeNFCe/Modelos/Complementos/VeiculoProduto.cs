// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record VeiculoProduto
{
    /// <summary>XML: <c>tpOp</c>.</summary>
    public string? TpOp { get; init; }

    /// <summary>XML: <c>chassi</c>.</summary>
    public string? Chassi { get; init; }

    /// <summary>XML: <c>cCor</c>.</summary>
    public string? CCor { get; init; }

    /// <summary>XML: <c>xCor</c>.</summary>
    public string? XCor { get; init; }

    /// <summary>XML: <c>pot</c>.</summary>
    public string? Pot { get; init; }

    /// <summary>XML: <c>cilin</c>.</summary>
    public string? Cilin { get; init; }

    /// <summary>XML: <c>pesoL</c>.</summary>
    public string? PesoL { get; init; }

    /// <summary>XML: <c>pesoB</c>.</summary>
    public string? PesoB { get; init; }

    /// <summary>XML: <c>nSerie</c>.</summary>
    public string? NSerie { get; init; }

    /// <summary>XML: <c>tpComb</c>.</summary>
    public string? TpComb { get; init; }

    /// <summary>XML: <c>nMotor</c>.</summary>
    public string? NMotor { get; init; }

    /// <summary>XML: <c>CMT</c>.</summary>
    public string? CMT { get; init; }

    /// <summary>XML: <c>dist</c>.</summary>
    public string? Dist { get; init; }

    /// <summary>XML: <c>anoMod</c>.</summary>
    public string? AnoMod { get; init; }

    /// <summary>XML: <c>anoFab</c>.</summary>
    public string? AnoFab { get; init; }

    /// <summary>XML: <c>tpPint</c>.</summary>
    public string? TpPint { get; init; }

    /// <summary>XML: <c>tpVeic</c>.</summary>
    public string? TpVeic { get; init; }

    /// <summary>XML: <c>espVeic</c>.</summary>
    public string? EspVeic { get; init; }

    /// <summary>XML: <c>VIN</c>.</summary>
    public string? VIN { get; init; }

    /// <summary>XML: <c>condVeic</c>.</summary>
    public string? CondVeic { get; init; }

    /// <summary>XML: <c>cMod</c>.</summary>
    public string? CMod { get; init; }

    /// <summary>XML: <c>cCorDENATRAN</c>.</summary>
    public string? CCorDENATRAN { get; init; }

    /// <summary>XML: <c>lota</c>.</summary>
    public string? Lota { get; init; }

    /// <summary>XML: <c>tpRest</c>.</summary>
    public string? TpRest { get; init; }
}
