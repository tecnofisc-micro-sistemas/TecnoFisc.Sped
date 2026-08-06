using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoC;

/// <summary>Registro C150 - período dos saldos periódicos.</summary>
[RegistroSped(Codigo = "C150", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC150 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C150";

    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }
}
