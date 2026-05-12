namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo de operação ISS — campo IND_OPER do Registro B020.
/// Exclusivo do Bloco B (ISS); não regido pelo Ato COTEPE/ICMS.
/// </summary>
public enum IndicadorOperacaoIss
{
    /// <summary>0 — Aquisição de serviço pelo declarante.</summary>
    Aquisicao = 0,

    /// <summary>1 — Prestação de serviço pelo declarante.</summary>
    Prestacao = 1,
}
