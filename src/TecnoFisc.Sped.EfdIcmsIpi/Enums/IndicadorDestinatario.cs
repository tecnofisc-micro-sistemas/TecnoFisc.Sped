namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do destinatário/acessante do documento fiscal — campo IND_DEST no Registro C500.
/// Obrigatório apenas nas operações de saída (IND_OPER = "1"); não apresentar nas entradas.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 136.
/// </summary>
public enum IndicadorDestinatario
{
    /// <summary>1 — Contribuinte do ICMS.</summary>
    ContribuinteIcms = 1,

    /// <summary>2 — Contribuinte Isento de Inscrição no Cadastro de Contribuintes do ICMS.</summary>
    IsentoInscricao = 2,

    /// <summary>9 — Não Contribuinte.</summary>
    NaoContribuinte = 9,
}
