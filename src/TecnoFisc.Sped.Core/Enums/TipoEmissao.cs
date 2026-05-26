namespace TecnoFisc.Sped.Core.Enums;

/// <summary>Forma de emissão da NF-e/NFC-e — campo tpEmis do grupo ide.</summary>
public enum TipoEmissao
{
    /// <summary>1 — Emissão normal.</summary>
    Normal = 1,

    /// <summary>2 — Contingência FS-IA (formulário de segurança).</summary>
    ContingenciaFsia = 2,

    /// <summary>3 — Contingência SCAN (desativado).</summary>
    ContingenciaScan = 3,

    /// <summary>4 — Contingência DPEC/EPEC.</summary>
    ContingenciaEpec = 4,

    /// <summary>5 — Contingência FS-DA.</summary>
    ContingenciaFsda = 5,

    /// <summary>6 — Contingência SVC-AN.</summary>
    ContingenciaSvcAn = 6,

    /// <summary>7 — Contingência SVC-RS.</summary>
    ContingenciaSvcRs = 7,

    /// <summary>9 — Contingência offline da NFC-e.</summary>
    ContingenciaOfflineNfce = 9,
}
