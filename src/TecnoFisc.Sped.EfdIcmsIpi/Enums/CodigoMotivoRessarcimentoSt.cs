namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Código do motivo do ressarcimento de ICMS ST — campo COD_MOT_RES do Registro C176.
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 86.
/// </summary>
public enum CodigoMotivoRessarcimentoSt
{
    /// <summary>1 — Saída para outra UF.</summary>
    SaidaParaOutraUf = 1,

    /// <summary>2 — Saída amparada por isenção ou não incidência.</summary>
    SaidaIsentaOuNaoIncidencia = 2,

    /// <summary>3 — Perda ou deterioração.</summary>
    PerdaOuDeterioracao = 3,

    /// <summary>4 — Furto ou roubo.</summary>
    FurtoOuRoubo = 4,

    /// <summary>5 — Exportação.</summary>
    Exportacao = 5,

    /// <summary>6 — Venda interna para Simples Nacional.</summary>
    VendaInternaParaSimplesNacional = 6,

    /// <summary>9 — Outros.</summary>
    Outros = 9,
}
