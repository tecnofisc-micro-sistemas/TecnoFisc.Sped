using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1600 — Contribuição Social Extemporânea – Cofins.
/// Nível hierárquico 2, ocorrência vários (por arquivo). Registra contribuição cujo período
/// de apuração anterior deveria ter sido escriturado mas somente agora é informado.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 403.
/// </summary>
[RegistroSped(Codigo = "1600", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1600 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1600";

    /// <summary>Período de apuração da contribuição social extemporânea no formato MMAAAA.</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? PerApurAnt { get; set; }

    /// <summary>Natureza da contribuição a recolher, conforme Tabela 4.3.5.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public CodigoContribuicaoSocialApurada NatContRec { get; set; }

    /// <summary>Valor da contribuição apurada, conforme detalhamento do Registro 1610.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContApur { get; set; }

    /// <summary>Valor do crédito de COFINS a descontar da contribuição social extemporânea, conforme Registro 1620.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredCofinsDesc { get; set; }

    /// <summary>Valor da contribuição social extemporânea devida (VL_CONT_APUR − VL_CRED_COFINS_DESC).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContDev { get; set; }

    /// <summary>Valor de outras deduções à contribuição social extemporânea devida.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOutDed { get; set; }

    /// <summary>Valor da contribuição social extemporânea a pagar (VL_CONT_DEV − VL_OUT_DED).</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContExt { get; set; }

    /// <summary>Valor da multa vinculada ao recolhimento da contribuição extemporânea a pagar.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? VlMul { get; set; }

    /// <summary>Valor dos juros vinculados ao recolhimento da contribuição extemporânea a pagar.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlJur { get; set; }

    /// <summary>Data do recolhimento da contribuição extemporânea a pagar.</summary>
    [CampoSped(Ordem = 11, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtRecol { get; set; }
}
