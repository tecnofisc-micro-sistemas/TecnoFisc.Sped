using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecf.Versionamento;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X371 - informações sobre ajustes compensatórios.</summary>
[RegistroSped(Codigo = "X371", Nivel = 3, Bloco = "X", IntroduzidoEm = (int)LayoutEcf.V010)]
public sealed partial class RegistroX371 : RegistroSped
{
    public override string Codigo => "X371";

    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "COD_CTA")]
    public string? CodCta { get; set; }

    [CampoSped(Ordem = 3, Nome = "COD_CCUS")]
    public string? CodCcus { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VALOR")]
    public decimal Valor { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true, Nome = "IND_VALOR")]
    public IndicadorDebitoCredito IndValor { get; set; }
}
