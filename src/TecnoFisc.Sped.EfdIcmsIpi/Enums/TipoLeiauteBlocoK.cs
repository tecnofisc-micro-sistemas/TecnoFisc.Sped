using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Tipo de leiaute adotado pelo contribuinte para o Bloco K — campo IND_TP_LEIAUTE do
/// Registro K010. Valores conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 266 (Subseção 12,
/// Ajuste Sinief 02/09; obrigatoriedade do registro a partir de 2023).
/// </summary>
public enum TipoLeiauteBlocoK
{
    /// <summary>0 — Leiaute simplificado.</summary>
    [SpedValor("0")]
    Simplificado = 0,

    /// <summary>1 — Leiaute completo.</summary>
    [SpedValor("1")]
    Completo = 1,

    /// <summary>2 — Leiaute restrito aos saldos de estoque.</summary>
    [SpedValor("2")]
    RestritoSaldosEstoque = 2,
}
