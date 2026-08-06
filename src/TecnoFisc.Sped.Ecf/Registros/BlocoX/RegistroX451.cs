using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecf.Versionamento;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X451 - demais informações dinâmicas de pagamentos e remessas.</summary>
[RegistroSped(Codigo = "X451", Nivel = 3, Bloco = "X", IntroduzidoEm = (int)LayoutEcf.V011)]
public sealed partial class RegistroX451 : RegistroSped
{
    public override string Codigo => "X451";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    [CampoSped(Ordem = 4)]
    public string? Valor { get; set; }
}
