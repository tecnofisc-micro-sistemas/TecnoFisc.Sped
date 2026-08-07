using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X354 - demonstrativo de prejuízos acumulados.</summary>
[RegistroSped(Codigo = "X354", Nivel = 3, Bloco = "X")]
public sealed partial class RegistroX354 : RegistroSped
{
    public override string Codigo => "X354";

    [CampoSped(Ordem = 2, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_NEG_ANT")]
    public decimal ResNegAnt { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "RES_NEG_ANT_REAL")]
    public decimal ResNegAntReal { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "SALDO_NEG_ACUM")]
    public decimal SaldoNegAcum { get; set; }
}
