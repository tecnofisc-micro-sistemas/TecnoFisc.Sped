using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoK;

/// <summary>
/// Registro K030 — Período da Escrituração Contábil Consolidada. Nível hierárquico 2, ocorrência
/// 0:1 por arquivo. Identifica o período da escrituração contábil consolidada. Conforme Manual
/// de Orientação do Leiaute 9 da ECD, p. 210–211.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_OBRIGATORIO_K030</c>: presente quando IND_ESC_CONS (Campo 20 do Registro 0000)
///   for "S" (Sim) e o mês da data final das informações do arquivo (DT_FIN do Registro 0000) for
///   igual a 12 (dezembro). Ausente caso contrário.</item>
///   <item><c>REGRA_DATA_MAIOR_INICIAL_CONS</c>: <see cref="DtFin"/> deve ser maior ou igual a
///   <see cref="DtIni"/>.</item>
///   <item><c>REGRA_IGUAL_DT_FIN_REG0000</c>: <see cref="DtFin"/> deve ser igual ao DT_FIN
///   (Campo 04) do Registro 0000.</item>
///   <item><c>REGRA_PERIODO_CONS</c>: a diferença entre <see cref="DtFin"/> e <see cref="DtIni"/>
///   deve ser menor ou igual a 1 (um) ano.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "K030", Nivel = 2, Bloco = "K")]
public sealed partial class RegistroK030 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K030";

    /// <summary>
    /// Data inicial do período consolidado (ddMMyyyy).
    /// Campo <c>DT_INI</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>
    /// Data final do período consolidado (ddMMyyyy).
    /// Campo <c>DT_FIN</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }
}
