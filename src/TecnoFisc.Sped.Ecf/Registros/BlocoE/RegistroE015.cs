using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoE;

/// <summary>Registro E015 - contas contábeis mapeadas.</summary>
[RegistroSped(Codigo = "E015", Nivel = 3, Bloco = "E")]
public sealed partial class RegistroE015 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E015";

    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true, Nome = "COD_CTA")]
    public string? CodCta { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Nome = "COD_CCUS")]
    public string? CodCcus { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true, Nome = "DESC_CTA")]
    public string? DescCta { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VAL_CTA")]
    public decimal ValCta { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 1, Obrigatorio = true, Nome = "IND_VAL_CTA")]
    public IndicadorDebitoCredito IndValCta { get; set; }
}
