using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1200 — Contribuição Social Extemporânea – PIS/Pasep.
/// Nível hierárquico 2, ocorrência vários (por arquivo). Controla contribuição social
/// extemporânea de período anterior, segregada por NAT_CONT_REC e DT_RECOL.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 391.
/// </summary>
[RegistroSped(Codigo = "1200", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1200 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1200";

    /// <summary>Período de apuração da contribuição social extemporânea no formato MMAAAA.</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? PerApurAnt { get; set; }

    /// <summary>Natureza da contribuição a recolher, conforme Tabela 4.3.5.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public CodigoContribuicaoSocialApurada NatContRec { get; set; }

    /// <summary>Valor da contribuição apurada.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContApur { get; set; }

    /// <summary>Valor do crédito de PIS/Pasep a descontar da contribuição social extemporânea.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredPisDesc { get; set; }

    /// <summary>Valor da contribuição social extemporânea devida (VL_CONT_APUR – VL_CRED_PIS_DESC).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContDev { get; set; }

    /// <summary>Valor de outras deduções.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOutDed { get; set; }

    /// <summary>Valor da contribuição social extemporânea a pagar (VL_CONT_DEV – VL_OUT_DED).</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContExt { get; set; }

    /// <summary>Valor da multa vinculada ao recolhimento da contribuição extemporânea.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? VlMul { get; set; }

    /// <summary>Valor dos juros vinculados ao recolhimento da contribuição extemporânea.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlJur { get; set; }

    /// <summary>Data do recolhimento (ddmmaaaa).</summary>
    [CampoSped(Ordem = 11, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtRecol { get; set; }
}
