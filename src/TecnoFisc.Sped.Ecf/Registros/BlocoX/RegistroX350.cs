using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X350 - resultado do período de apuração da participação no exterior.</summary>
[RegistroSped(Codigo = "X350", Nivel = 3, Bloco = "X")]
public sealed partial class RegistroX350 : RegistroSped
{
    public override string Codigo => "X350";

    [CampoSped(Ordem = 2, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal RecLiq { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal Custos { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal LucBruto { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal RecAuferidas { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal RecOutrasOper { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal DespBrasil { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal DespOper { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal LucOper { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal RecPartic { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal RecOutras { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal DespOutras { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal LucLiqAntIr { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal LucArbAntIr { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ImpDev { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal LucLiq { get; set; }
}
