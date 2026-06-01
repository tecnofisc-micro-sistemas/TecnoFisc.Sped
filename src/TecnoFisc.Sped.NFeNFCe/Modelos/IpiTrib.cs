namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>IPITrib</c> — IPI tributado (CST 00, 49, 50 ou 99).
/// Contém os campos de cálculo ad valorem (<c>vBC</c>/<c>pIPI</c>) ou específico
/// (<c>qUnid</c>/<c>vUnid</c>), mais o valor do imposto apurado (<c>vIPI</c>).
/// </summary>
public sealed record IpiTrib : IpiTributacao
{
    // -------------------------------------------------------------------------
    // Ad valorem (XSD <xs:choice> — par vBC/pIPI ou par qUnid/vUnid, mutuamente exclusivos;
    // modelados como nullable para suportar qualquer combinação sem pos-reads.
    // -------------------------------------------------------------------------

    /// <summary><c>vBC</c> — valor da base de cálculo do IPI (ad valorem, opcional).</summary>
    public decimal? VBC { get; init; }

    /// <summary><c>pIPI</c> — alíquota do IPI em % (ad valorem, opcional).</summary>
    public decimal? PIPI { get; init; }

    // -------------------------------------------------------------------------
    // Específico
    // -------------------------------------------------------------------------

    /// <summary><c>qUnid</c> — quantidade total na unidade padrão para tributação (específico, opcional).</summary>
    public decimal? QUnid { get; init; }

    /// <summary><c>vUnid</c> — valor do imposto por unidade tributável (específico, opcional).</summary>
    public decimal? VUnid { get; init; }

    // -------------------------------------------------------------------------
    // Campo obrigatório
    // -------------------------------------------------------------------------

    /// <summary><c>vIPI</c> — valor do IPI apurado.</summary>
    public required decimal VIPI { get; init; }
}
