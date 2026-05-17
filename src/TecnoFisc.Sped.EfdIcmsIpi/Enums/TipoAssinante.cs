namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Código do tipo de assinante — campo TP_ASSINANTE do Registro D500 (NF Serviço de
/// Comunicação cód. 21 e Telecomunicação cód. 22).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 192.
/// </summary>
public enum TipoAssinante
{
    /// <summary>1 — Comercial/Industrial.</summary>
    ComercialIndustrial = 1,

    /// <summary>2 — Poder Público.</summary>
    PoderPublico = 2,

    /// <summary>3 — Residencial/Pessoa física.</summary>
    ResidencialPessoaFisica = 3,

    /// <summary>4 — Público.</summary>
    Publico = 4,

    /// <summary>5 — Semi-Público.</summary>
    SemiPublico = 5,

    /// <summary>6 — Outros.</summary>
    Outros = 6,
}
