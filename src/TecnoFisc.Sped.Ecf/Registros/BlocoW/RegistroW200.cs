using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoW;

/// <summary>Registro W200 - valores agregados da Declaração País-a-País.</summary>
[RegistroSped(Codigo = "W200", Nivel = 3, Bloco = "W")]
public sealed partial class RegistroW200 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "W200";

    /// <summary>Jurisdição de residência conforme tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true, Nome = "JURISDICAO")]
    public string? Jurisdicao { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 0, Nome = "VL_REC_NAO_REL_EST")]
    public decimal? VlRecNaoRelEst { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 0, Obrigatorio = true, Nome = "VL_REC_NAO_REL")]
    public decimal VlRecNaoRel { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 0, Nome = "VL_REC_REL_EST")]
    public decimal? VlRecRelEst { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 0, Obrigatorio = true, Nome = "VL_REC_REL")]
    public decimal VlRecRel { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 0, Nome = "VL_REC_TOTAL_EST")]
    public decimal? VlRecTotalEst { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 0, Obrigatorio = true, Nome = "VL_REC_TOTAL")]
    public decimal VlRecTotal { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 19, Decimais = 0, Nome = "VL_LUC_PREJ_ANTES_IR_EST")]
    public decimal? VlLucPrejAntesIrEst { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 0, Obrigatorio = true, Nome = "VL_LUC_PREJ_ANTES_IR")]
    public decimal VlLucPrejAntesIr { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 19, Decimais = 0, Nome = "VL_IR_PAGO_EST")]
    public decimal? VlIrPagoEst { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 19, Decimais = 0, Obrigatorio = true, Nome = "VL_IR_PAGO")]
    public decimal VlIrPago { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 19, Decimais = 0, Nome = "VL_IR_DEVIDO_EST")]
    public decimal? VlIrDevidoEst { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 19, Decimais = 0, Obrigatorio = true, Nome = "VL_IR_DEVIDO")]
    public decimal VlIrDevido { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 19, Decimais = 0, Nome = "VL_CAP_SOC_EST")]
    public decimal? VlCapSocEst { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 19, Decimais = 0, Obrigatorio = true, Nome = "VL_CAP_SOC")]
    public decimal VlCapSoc { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 19, Decimais = 0, Nome = "VL_LUC_ACUM_EST")]
    public decimal? VlLucAcumEst { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 19, Decimais = 0, Obrigatorio = true, Nome = "VL_LUC_ACUM")]
    public decimal VlLucAcum { get; set; }

    [CampoSped(Ordem = 19, Tamanho = 19, Decimais = 0, Nome = "VL_ATIV_TANG_EST")]
    public decimal? VlAtivTangEst { get; set; }

    [CampoSped(Ordem = 20, Tamanho = 19, Decimais = 0, Obrigatorio = true, Nome = "VL_ATIV_TANG")]
    public decimal VlAtivTang { get; set; }

    /// <summary>Número de empregados.</summary>
    [CampoSped(Ordem = 21, Tamanho = 7, Decimais = 0, Obrigatorio = true, Nome = "NUM_EMP")]
    public int NumEmp { get; set; }
}
