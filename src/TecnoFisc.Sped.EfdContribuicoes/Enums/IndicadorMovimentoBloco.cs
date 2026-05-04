namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador de movimento de um bloco — campo IND_MOV dos registros de abertura
/// (X001). Valores conforme Guia Prático v1.35: <c>0 - Bloco com dados informados</c>
/// e <c>1 - Bloco sem dados informados</c>. O guia ora declara o campo como N, ora como
/// C, mas o domínio de valores é idêntico em todos os blocos.
/// </summary>
public enum IndicadorMovimentoBloco
{
    /// <summary>Bloco contém registros de movimento além das aberturas/encerramentos.</summary>
    ComDados = 0,

    /// <summary>Bloco sem registros de movimento — apenas abertura e encerramento.</summary>
    SemDados = 1,
}
