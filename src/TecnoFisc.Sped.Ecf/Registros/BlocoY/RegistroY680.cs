using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y680 - mês das informações de optantes pelo Refis.</summary>
[RegistroSped(Codigo = "Y680", Nivel = 2, Bloco = "Y")]
public sealed partial class RegistroY680 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y680";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public MesCalendarioEcf Mes { get; set; }
}
