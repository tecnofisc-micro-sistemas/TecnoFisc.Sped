namespace TecnoFisc.Sped.Core.Enums;

/// <summary>Finalidade de emissão da NF-e/NFC-e — campo finNFe do grupo ide.</summary>
public enum FinalidadeEmissao
{
    /// <summary>1 — NF-e normal.</summary>
    Normal = 1,

    /// <summary>2 — NF-e complementar.</summary>
    Complementar = 2,

    /// <summary>3 — NF-e de ajuste.</summary>
    Ajuste = 3,

    /// <summary>4 — Devolução/Retorno.</summary>
    Devolucao = 4,

    /// <summary>5 — Nota de crédito.</summary>
    NotaCredito = 5,

    /// <summary>6 — Nota de débito.</summary>
    NotaDebito = 6,
}
