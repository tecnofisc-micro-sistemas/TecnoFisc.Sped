namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do local de execução do serviço — campo LOC_EXE_SERV do Registro A120.
/// Valores conforme Guia Prático v1.35, p. 100.
/// </summary>
public enum IndicadorLocalExecucaoServico
{
    /// <summary>0 — Executado no País.</summary>
    Pais = 0,

    /// <summary>1 — Executado no Exterior, cujo resultado se verifique no País.</summary>
    Exterior = 1,
}
