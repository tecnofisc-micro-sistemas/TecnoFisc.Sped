using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoW;

/// <summary>Registro W300 - observações adicionais da Declaração País-a-País.</summary>
[RegistroSped(Codigo = "W300", Nivel = 2, Bloco = "W")]
public sealed partial class RegistroW300 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "W300";

    [CampoSped(Ordem = 2, Nome = "JURISDICAO")]
    public string? Jurisdicao { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1, Nome = "IND_REC_NAO_REL")]
    public IndicadorSimNao? IndRecNaoRel { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 1, Nome = "IND_REC_REL")]
    public IndicadorSimNao? IndRecRel { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 1, Nome = "IND_REC_TOTAL")]
    public IndicadorSimNao? IndRecTotal { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 1, Nome = "IND_LUC_PREJ_ANTES_IR")]
    public IndicadorSimNao? IndLucPrejAntesIr { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 1, Nome = "IND_IR_PAGO")]
    public IndicadorSimNao? IndIrPago { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 1, Nome = "IND_IR_DEVIDO")]
    public IndicadorSimNao? IndIrDevido { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 1, Nome = "IND_CAP_SOC")]
    public IndicadorSimNao? IndCapSoc { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 1, Nome = "IND_LUC_ACUM")]
    public IndicadorSimNao? IndLucAcum { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 1, Nome = "IND_ATIV_TANG")]
    public IndicadorSimNao? IndAtivTang { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 1, Nome = "IND_NUM_EMP")]
    public IndicadorSimNao? IndNumEmp { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 4000, Obrigatorio = true, Nome = "OBSERVAÇÃO")]
    public string? Observação { get; set; }

    /// <summary>Marcador fixo W300FIM, obrigatório conforme o marcador normativo "sim".</summary>
    [CampoSped(Ordem = 14, Tamanho = 7, Obrigatorio = true, Nome = "FIM_OBSERVACAO")]
    public string? FimObservacao { get; set; }
}
