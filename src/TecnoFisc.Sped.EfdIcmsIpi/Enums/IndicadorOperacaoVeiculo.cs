namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo de operação com veículo novo — campo IND_VEIC_OPER no Registro C175
/// (Operações com Veículos Novos, cód. 01 e 55).
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 84.
/// </summary>
public enum IndicadorOperacaoVeiculo
{
    /// <summary>0 — Venda para concessionária.</summary>
    VendaParaConcessionaria = 0,

    /// <summary>1 — Faturamento direto.</summary>
    FaturamentoDireto = 1,

    /// <summary>2 — Venda direta.</summary>
    VendaDireta = 2,

    /// <summary>3 — Venda da concessionária.</summary>
    VendaDaConcessionaria = 3,

    /// <summary>9 — Outros.</summary>
    Outros = 9,
}
