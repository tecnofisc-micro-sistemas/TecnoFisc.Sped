using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoC;

/// <summary>Registro C050 - plano de contas da ECD.</summary>
[RegistroSped(Codigo = "C050", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC050 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C050";

    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtAlt { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public CodigoNaturezaContaContabil CodNat { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoConta IndCta { get; set; }

    [CampoSped(Ordem = 5, Nome = "NÍVEL", Tamanho = 0, Obrigatorio = true)]
    public int Nível { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 0, Obrigatorio = true)]
    public string? CodCta { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0)]
    public string? CodCtaSup { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0, Obrigatorio = true)]
    public string? Cta { get; set; }
}
