using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M200 — Consolidação da Contribuição para o PIS/Pasep do Período.
/// Nível hierárquico 2, ocorrência um por arquivo.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 305.
/// </summary>
[RegistroSped(Codigo = "M200", Nivel = 2, Bloco = "M")]
public sealed partial class RegistroM200 : RegistroSped
{
    public override string Codigo => "M200";

    /// <summary>Valor Total da Contribuição Não Cumulativa do Período.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotContNcPer { get; set; }

    /// <summary>Valor do Crédito Descontado, Apurado no Próprio Período da Escrituração.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotCredDesc { get; set; }

    /// <summary>Valor do Crédito Descontado, Apurado em Período de Apuração Anterior.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotCredDescAnt { get; set; }

    /// <summary>Valor Total da Contribuição Não Cumulativa Devida (02 – 03 – 04).</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotContNcDev { get; set; }

    /// <summary>Valor Retido na Fonte Deduzido no Período (regime não cumulativo).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRetNc { get; set; }

    /// <summary>Outras Deduções no Período (regime não cumulativo).</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOutDedNc { get; set; }

    /// <summary>Valor da Contribuição Não Cumulativa a Recolher/Pagar (05 – 06 – 07).</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContNcRec { get; set; }

    /// <summary>Valor Total da Contribuição Cumulativa do Período.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotContCumPer { get; set; }

    /// <summary>Valor Retido na Fonte Deduzido no Período (regime cumulativo).</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRetCum { get; set; }

    /// <summary>Outras Deduções no Período (regime cumulativo).</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOutDedCum { get; set; }

    /// <summary>Valor da Contribuição Cumulativa a Recolher/Pagar (09 – 10 – 11).</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContCumRec { get; set; }

    /// <summary>Valor Total da Contribuição a Recolher/Pagar no Período (08 + 12).</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotContRec { get; set; }
}
