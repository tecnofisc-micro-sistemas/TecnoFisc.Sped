using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoC;

/// <summary>Registro C040 - identificação da ECD recuperada.</summary>
[RegistroSped(Codigo = "C040", Nivel = 2, Bloco = "C")]
public sealed partial class RegistroC040 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C040";

    [CampoSped(Ordem = 2, Tamanho = 40, Obrigatorio = true)]
    public string? HashEcd { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 1)]
    public string? IndSitEsp { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Obrigatorio = true)]
    public int NumOrd { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 11)]
    public string? Nire { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 80, Obrigatorio = true)]
    public string? NatLivr { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 0, Obrigatorio = true)]
    public string? CodVerLc { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 1, Obrigatorio = true)]
    public string? IndEsc { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IdentMf { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndEscCons { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 1, Obrigatorio = true)]
    public string? IndCentralizada { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 1, Obrigatorio = true)]
    public string? IndMudancPc { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 2)]
    public string? CodPlanRef { get; set; }
}
