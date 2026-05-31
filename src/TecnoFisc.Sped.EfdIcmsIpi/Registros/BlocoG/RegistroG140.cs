using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoG;

/// <summary>
/// Registro G140 - Identificacao do item do documento fiscal.
/// Nivel hierarquico 5, ocorrencia varios por Registro G130. Conforme Guia Pratico
/// EFD-ICMS/IPI V3.0.6, p. 243-244.
/// </summary>
[RegistroSped(Codigo = "G140", Nivel = 5, Bloco = "G")]
public sealed partial class RegistroG140 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "G140";

    /// <summary>Numero sequencial do item no documento fiscal.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public int? NumItem { get; set; }

    /// <summary>Codigo correspondente do bem no documento fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade deste item da nota fiscal aplicada neste bem.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 5, Obrigatorio = true)]
    public decimal? Qtde { get; set; }

    /// <summary>Unidade do item constante no documento fiscal de entrada.</summary>
    [CampoSped(Ordem = 5, Tamanho = 6, Obrigatorio = true)]
    public string? Unid { get; set; }

    /// <summary>Valor do ICMS da operacao propria aplicado ao bem ou componente.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlIcmsOpAplicado { get; set; }

    /// <summary>Valor do ICMS ST aplicado ao bem ou componente.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlIcmsStAplicado { get; set; }

    /// <summary>Valor do ICMS sobre frete aplicado ao bem ou componente.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlIcmsFrtAplicado { get; set; }

    /// <summary>Valor do ICMS diferencial de aliquota aplicado ao bem ou componente.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlIcmsDifAplicado { get; set; }
}
