using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X351 - resultados e imposto pago no exterior.</summary>
[RegistroSped(Codigo = "X351", Nivel = 3, Bloco = "X")]
public sealed partial class RegistroX351 : RegistroSped
{
    public override string Codigo => "X351";

    [CampoSped(Ordem = 2, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ResInvPer { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ResInvPerReal { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ResIsenPetrPer { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ResIsenPetrPerReal { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ResNegAcum { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ResNegAcumReal { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ResPosTrib { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ResPosTribReal { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ImpLucr { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ImpLucrReal { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ImpPagRend { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ImpPagRendReal { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ImpRetExt { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ImpRetExtReal { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal ImpRetBr { get; set; }
}
