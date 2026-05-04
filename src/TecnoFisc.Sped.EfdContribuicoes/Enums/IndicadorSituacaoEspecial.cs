namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador de situação especial — campo IND_SIT_ESP do Registro 0000. Apenas preenchido
/// quando a escrituração se refere a um evento de abertura, cisão, fusão, incorporação ou
/// encerramento da pessoa jurídica.
/// </summary>
public enum IndicadorSituacaoEspecial
{
    Abertura = 0,
    Cisao = 1,
    Fusao = 2,
    Incorporacao = 3,
    Encerramento = 4,
}
