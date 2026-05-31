using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0110 — Regimes de Apuração da Contribuição Social e de Apropriação de Crédito.
/// Nível hierárquico 2, ocorrência um (por arquivo). Conforme Guia Prático v1.35, p. 71-73.
/// </summary>
[RegistroSped(Codigo = "0110", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0110 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0110";

    /// <summary>Código indicador da incidência tributária no período (1=não-cumulativo; 2=cumulativo; 3=ambos).</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public CodigoIncidenciaTributaria CodIncTrib { get; set; }

    /// <summary>
    /// Indicador do método de apropriação de créditos comuns, no caso de incidência no regime
    /// não-cumulativo (COD_INC_TRIB = 1 ou 3): 1=Apropriação Direta; 2=Rateio Proporcional.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 1)]
    public IndicadorApropriacaoCredito? IndAproCred { get; set; }

    /// <summary>
    /// Código indicador do tipo de contribuição apurada no período:
    /// 1=Alíquota Básica; 2=Alíquotas Específicas (diferenciadas e/ou por unidade de medida).
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 1)]
    public CodigoTipoContribuicao? CodTipoCont { get; set; }

    /// <summary>
    /// Indicador do critério de escrituração no regime cumulativo para lucro presumido
    /// (usado quando COD_INC_TRIB = 2): 1=Regime de Caixa; 2=Competência consolidada; 9=Competência detalhada.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 1)]
    public IndicadorRegimeCumulativo? IndRegCum { get; set; }
}
