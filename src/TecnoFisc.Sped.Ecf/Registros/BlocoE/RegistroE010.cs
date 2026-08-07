using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoE;

/// <summary>Registro E010 - saldos finais recuperados da ECF anterior.</summary>
[RegistroSped(Codigo = "E010", Nivel = 2, Bloco = "E")]
public sealed partial class RegistroE010 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E010";

    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true, Nome = "COD_NAT")]
    public string? CodNat { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true, Nome = "COD_CTA_REF")]
    public string? CodCtaRef { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true, Nome = "DESC_CTA_REF")]
    public string? DescCtaRef { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VAL_CTA_REF")]
    public decimal ValCtaRef { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 1, Obrigatorio = true, Nome = "IND_VAL_CTA_REF")]
    public IndicadorDebitoCredito IndValCtaRef { get; set; }
}
