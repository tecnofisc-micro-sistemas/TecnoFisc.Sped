using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M300 — Contribuição de PIS/Pasep Diferida em Períodos Anteriores – Valores a Pagar no Período.
/// Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 324.
/// </summary>
[RegistroSped(Codigo = "M300", Nivel = 2, Bloco = "M")]
public sealed partial class RegistroM300 : RegistroSped
{
    public override string Codigo => "M300";

    /// <summary>Código da contribuição social diferida em períodos anteriores, conforme Tabela 4.3.5.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public CodigoContribuicaoSocialApurada CodCont { get; set; }

    /// <summary>Valor da Contribuição Apurada, diferida em períodos anteriores.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContApurDifer { get; set; }

    /// <summary>Natureza do Crédito Diferido vinculado à receita tributada no mercado interno.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2)]
    public NaturezaCreditoDiferido? NatCredDesc { get; set; }

    /// <summary>Valor do Crédito a Descontar vinculado à contribuição diferida.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlCredDescDifer { get; set; }

    /// <summary>Valor da Contribuição a Recolher, diferida em períodos anteriores (Campo 03 – Campo 05).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContDiferAnt { get; set; }

    /// <summary>Período de apuração da contribuição social e dos créditos diferidos (MMAAAA).</summary>
    [CampoSped(Ordem = 7, Tamanho = 6, Obrigatorio = true)]
    public string? PerApur { get; set; }

    /// <summary>Data de recebimento da receita, objeto de diferimento (ddmmaaaa).</summary>
    [CampoSped(Ordem = 8, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtReceb { get; set; }
}
