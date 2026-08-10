using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X355 - demonstrativo de rendas ativas e passivas.</summary>
[RegistroSped(Codigo = "X355", Nivel = 3, Bloco = "X")]
public sealed partial class RegistroX355 : RegistroSped
{
    public override string Codigo => "X355";

    [CampoSped(Ordem = 2, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "REND_PASS_PROP")]
    public decimal RendPassProp { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "REND_PASS_PROP_REAL")]
    public decimal RendPassPropReal { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "REND_TOTAL")]
    public decimal RendTotal { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "REND_TOTAL_REAL")]
    public decimal RendTotalReal { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "REND_ATIV_PROP")]
    public decimal RendAtivProp { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "REND_ATIV_PROP_REAL")]
    public decimal RendAtivPropReal { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 8, Decimais = 4, Obrigatorio = true, Nome = "PERCENTUAL")]
    public decimal Percentual { get; set; }
}
