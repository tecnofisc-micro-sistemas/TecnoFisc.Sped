using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X350 - resultado do período de apuração da participação no exterior.</summary>
[RegistroSped(Codigo = "X350", Nivel = 3, Bloco = "X")]
public sealed partial class RegistroX350 : RegistroSped
{
    public override string Codigo => "X350";

    [CampoSped(Ordem = 2, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "REC_LIQ")]
    public decimal RecLiq { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "CUSTOS")]
    public decimal Custos { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "LUC_BRUTO")]
    public decimal LucBruto { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "REC_AUFERIDAS")]
    public decimal RecAuferidas { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "REC_OUTRAS_OPER")]
    public decimal RecOutrasOper { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "DESP_BRASIL")]
    public decimal DespBrasil { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "DESP_OPER")]
    public decimal DespOper { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "LUC_OPER")]
    public decimal LucOper { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "REC_PARTIC")]
    public decimal RecPartic { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "REC_OUTRAS")]
    public decimal RecOutras { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "DESP_OUTRAS")]
    public decimal DespOutras { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "LUC_LIQ_ANT_IR")]
    public decimal LucLiqAntIr { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "LUC_ARB_ANT_IR")]
    public decimal LucArbAntIr { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "IMP_DEV")]
    public decimal ImpDev { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "LUC_LIQ")]
    public decimal LucLiq { get; set; }
}
