using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y682 - informações mensais dos optantes pelo Refis imunes ou isentos.</summary>
[RegistroSped(Codigo = "Y682", Nivel = 2, Bloco = "Y")]
public sealed partial class RegistroY682 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y682";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public MesCalendarioEcf Mes { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal AcresPatr { get; set; }
}
