// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.
namespace TecnoFisc.Sped.NFeNFCe;

public sealed partial record NFCe
{
    /// <summary>XML: <c>avulsa</c>.</summary>
    public InfNFeAvulsa? Avulsa { get; init; }

    /// <summary>XML: <c>retirada</c>.</summary>
    public LocalRetirada? Retirada { get; init; }

    /// <summary>XML: <c>entrega</c>.</summary>
    public LocalEntrega? Entrega { get; init; }

    /// <summary>XML: <c>autXML</c>.</summary>
    public IReadOnlyList<AutorizadoXml> AutXML { get; init; } = [];

    /// <summary>XML: <c>transp</c>.</summary>
    public Transporte? Transp { get; init; }

    /// <summary>XML: <c>cobr</c>.</summary>
    public Cobranca? Cobr { get; init; }

    /// <summary>XML: <c>pag</c>.</summary>
    public Pagamento? Pag { get; init; }

    /// <summary>XML: <c>infIntermed</c>.</summary>
    public Intermediador? InfIntermed { get; init; }

    /// <summary>XML: <c>infAdic</c>.</summary>
    public InformacoesAdicionais? InfAdic { get; init; }

    /// <summary>XML: <c>exporta</c>.</summary>
    public Exportacao? Exporta { get; init; }

    /// <summary>XML: <c>compra</c>.</summary>
    public Compra? Compra { get; init; }

    /// <summary>XML: <c>cana</c>.</summary>
    public Cana? Cana { get; init; }

    /// <summary>XML: <c>infRespTec</c>.</summary>
    public ResponsavelTecnico? InfRespTec { get; init; }

    /// <summary>XML: <c>infSolicNFF</c>.</summary>
    public InfNFeInfSolicNFF? InfSolicNFF { get; init; }

    /// <summary>XML: <c>agropecuario</c>.</summary>
    public InfNFeAgropecuario? Agropecuario { get; init; }

    /// <summary>XML: <c>infPAA</c>.</summary>
    public InfNFeInfPAA? InfPAA { get; init; }
}
