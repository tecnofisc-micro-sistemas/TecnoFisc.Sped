// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.
namespace TecnoFisc.Sped.NFeNFCe;

public sealed partial record Imposto
{
    /// <summary>XML: <c>ICMSUFDest</c>.</summary>
    public IcmsUfDest? ICMSUFDest { get; init; }

    /// <summary>XML: <c>IS</c>.</summary>
    public ImpostoSeletivo? IS { get; init; }

    /// <summary>XML: <c>IBSCBS</c>.</summary>
    public TributacaoIBSCBS? IBSCBS { get; init; }
}
