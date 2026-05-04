namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do tipo de escrituração — campo TIPO_ESCRIT do Registro 0000. Valores válidos
/// pelo Guia Prático v1.35: <c>0 - Original</c> e <c>1 - Retificadora</c>.
/// </summary>
public enum TipoEscrituracao
{
    /// <summary>Escrituração original.</summary>
    Original = 0,

    /// <summary>Escrituração retificadora — exige preenchimento de NUM_REC_ANTERIOR.</summary>
    Retificadora = 1,
}
