namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do tipo de unidade imobiliária vendida — campo UNID_IMOB do Registro F200.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 250.
/// </summary>
public enum IndicadorUnidadeImobiliaria
{
    /// <summary>01 — Terreno adquirido para venda.</summary>
    TerrenoAdquiridoParaVenda = 1,

    /// <summary>02 — Terreno decorrente de loteamento.</summary>
    TerrenoLoteamento = 2,

    /// <summary>03 — Lote oriundo de desmembramento de terreno.</summary>
    LoteDesmembramento = 3,

    /// <summary>04 — Unidade resultante de incorporação imobiliária.</summary>
    UnidadeIncorporacao = 4,

    /// <summary>05 — Prédio construído/em construção para venda.</summary>
    PredioVenda = 5,

    /// <summary>06 — Outras.</summary>
    Outras = 6,
}
