namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo de referência da base de cálculo do ICMS-ST do produto farmacêutico —
/// campo IND_MED no Registro C173 (Operações com Medicamentos, cód. 01 e 55).
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 83.
/// </summary>
public enum IndicadorBaseMedicamento
{
    /// <summary>0 — Base de cálculo referente ao preço tabelado ou preço máximo sugerido.</summary>
    PrecoTabeladoOuMaximoSugerido = 0,

    /// <summary>1 — Base de cálculo — Margem de Valor Agregado.</summary>
    MargemValorAgregado = 1,

    /// <summary>2 — Base de cálculo referente à Lista Negativa.</summary>
    ListaNegativa = 2,

    /// <summary>3 — Base de cálculo referente à Lista Positiva.</summary>
    ListaPositiva = 3,

    /// <summary>4 — Base de cálculo referente à Lista Neutra.</summary>
    ListaNeutra = 4,
}
