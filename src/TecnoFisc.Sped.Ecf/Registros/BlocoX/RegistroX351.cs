using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X351 - resultados e imposto pago no exterior.</summary>
[RegistroSped(Codigo = "X351", Nivel = 3, Bloco = "X")]
public sealed partial class RegistroX351 : RegistroSped
{
    public override string Codigo => "X351";

    [CampoSped(Ordem = 2, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_INV_PER")]
    public decimal ResInvPer { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_INV_PER_REAL")]
    public decimal ResInvPerReal { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_ISEN_PETR_PER")]
    public decimal ResIsenPetrPer { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_ISEN_PETR_PER_REAL")]
    public decimal ResIsenPetrPerReal { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_NEG_ACUM")]
    public decimal ResNegAcum { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_NEG_ACUM_REAL")]
    public decimal ResNegAcumReal { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_POS_TRIB")]
    public decimal ResPosTrib { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_POS_TRIB_REAL")]
    public decimal ResPosTribReal { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "IMP_LUCR")]
    public decimal ImpLucr { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "IMP_LUCR_REAL")]
    public decimal ImpLucrReal { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "IMP_PAG_REND")]
    public decimal ImpPagRend { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "IMP_PAG_REND_REAL")]
    public decimal ImpPagRendReal { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "IMP_RET_EXT")]
    public decimal ImpRetExt { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "IMP_RET_EXT_REAL")]
    public decimal ImpRetExtReal { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "IMP_RET_BR")]
    public decimal ImpRetBr { get; set; }
}
