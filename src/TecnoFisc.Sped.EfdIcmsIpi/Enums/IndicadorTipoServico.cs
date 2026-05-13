namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo de serviço prestado — campo IND_SERV do Registro D530 (Terminal Faturado).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 202.
/// </summary>
public enum IndicadorTipoServico
{
    /// <summary>0 — Telefonia.</summary>
    Telefonia = 0,

    /// <summary>1 — Comunicação de dados.</summary>
    ComunicacaoDeDados = 1,

    /// <summary>2 — TV por assinatura.</summary>
    TvPorAssinatura = 2,

    /// <summary>3 — Provimento de acesso à Internet.</summary>
    ProvimentoAcessoInternet = 3,

    /// <summary>4 — Multimídia.</summary>
    Multimidia = 4,

    /// <summary>9 — Outros.</summary>
    Outros = 9,
}
