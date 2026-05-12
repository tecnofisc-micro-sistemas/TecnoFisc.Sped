namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Tipo de produto farmacêutico — campo TP_PROD no Registro C173
/// (Operações com Medicamentos, cód. 01 e 55).
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 83.
/// </summary>
public enum TipoProdutoFarmaceutico
{
    /// <summary>0 — Similar.</summary>
    Similar = 0,

    /// <summary>1 — Genérico.</summary>
    Generico = 1,

    /// <summary>2 — Ético ou de marca (medicamento de referência).</summary>
    EticoOuDeMarca = 2,
}
