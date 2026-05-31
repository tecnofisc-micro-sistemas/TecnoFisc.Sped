using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I150 — Saldos Periódicos – Identificação do Período. Nível hierárquico 3, ocorrência 0:N por I010.
/// Identifica o período relativo aos saldos contábeis. A periodicidade é no máximo mensal, podendo ter fração
/// de mês em situações especiais (cisão, fusão, incorporação ou extinção).
/// Obrigatório para escrituração do tipo G, R ou B (conforme <c>IND_ESC</c> do Registro I010).
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 131–133.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_CONTINUIDADE_SALDOS_PERIODICOS</c>: se existir pelo menos um I150, deve haver
///   I155 para todos os meses do intervalo informado no Registro 0000.</item>
///   <item><c>REGRA_DATA_MES</c>: <see cref="DtIni"/> e <see cref="DtFin"/> devem estar no mesmo mês.</item>
///   <item><c>REGRA_DUPLICIDADE_PERIODO_SALDO_PERIODICO</c>: a chave <see cref="DtIni"/>+<see cref="DtFin"/>
///   não pode ser duplicada e não pode haver mais de um I150 para o mesmo mês.</item>
///   <item><c>REGRA_VALIDA_MES_I157</c>: caso exista registro I157, o mês de <see cref="DtIni"/>
///   deve ser igual ao mês de DT_INI do Registro 0000.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I150", Nivel = 3, Bloco = "I")]
public sealed partial class RegistroI150 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I150";

    /// <summary>
    /// Data de início do período (ddMMyyyy).
    /// Campo <c>DT_INI</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>
    /// Data de fim do período (ddMMyyyy).
    /// Campo <c>DT_FIN</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }
}
