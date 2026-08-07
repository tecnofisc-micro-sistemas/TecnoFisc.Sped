using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X353 - demonstrativo de consolidação.</summary>
[RegistroSped(Codigo = "X353", Nivel = 3, Bloco = "X")]
public sealed partial class RegistroX353 : RegistroSped
{
    public override string Codigo => "X353";

    [CampoSped(Ordem = 2, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_NEG_UTIL")]
    public decimal ResNegUtil { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_NEG_UTIL_REAL")]
    public decimal ResNegUtilReal { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "SALDO_RES_NEG_NAO_UTIL")]
    public decimal SaldoResNegNaoUtil { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "SALDO_RES_NEG_NAO_UTIL_REAL")]
    public decimal SaldoResNegNaoUtilReal { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_PROP")]
    public decimal ResProp { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_PROP_REAL")]
    public decimal ResPropReal { get; set; }
}
