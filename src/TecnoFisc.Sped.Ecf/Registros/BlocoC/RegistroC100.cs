using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoC;

/// <summary>Registro C100 - centro de custos.</summary>
[RegistroSped(Codigo = "C100", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C100";

    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtAlt { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? CodCcus { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true)]
    public string? Ccus { get; set; }
}
