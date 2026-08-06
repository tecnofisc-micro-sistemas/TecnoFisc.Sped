using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X356 - demonstrativo da estrutura societária.</summary>
[RegistroSped(Codigo = "X356", Nivel = 3, Bloco = "X")]
public sealed partial class RegistroX356 : RegistroSped
{
    public override string Codigo => "X356";

    [CampoSped(Ordem = 2, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal PercPart { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal AtivoTotal { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal PatLiquido { get; set; }
}
