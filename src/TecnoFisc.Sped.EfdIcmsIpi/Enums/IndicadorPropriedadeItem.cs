using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador de propriedade ou posse do item — campo IND_PROP do Registro H010.
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 247.
/// </summary>
public enum IndicadorPropriedadeItem
{
    /// <summary>0 — Item de propriedade do informante e em seu poder.</summary>
    [SpedValor("0")]
    PropriedadeInformanteEmSeuPoder = 0,

    /// <summary>1 — Item de propriedade do informante em posse de terceiros.</summary>
    [SpedValor("1")]
    PropriedadeInformantePosseTerceiros = 1,

    /// <summary>2 — Item de propriedade de terceiros em posse do informante.</summary>
    [SpedValor("2")]
    PropriedadeTerceirosPosseInformante = 2,
}
