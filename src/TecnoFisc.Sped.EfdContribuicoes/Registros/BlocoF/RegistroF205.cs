using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F205 — Operações da Atividade Imobiliária – Custo Incorrido da Unidade Imobiliária.
/// Nível hierárquico 4, ocorrência 1:1 (filho de F200).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 252.
/// </summary>
[RegistroSped(Codigo = "F205", Nivel = 4, Bloco = "F")]
public sealed partial class RegistroF205 : RegistroSped
{
    public override string Codigo => "F205";

    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCusIncAcumAnt { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCusIncPerEsc { get; set; }

    /// <summary>Valor Total do Custo Incorrido acumulado até o mês da escrituração (Campo 02 + 03).</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCusIncAcum { get; set; }

    /// <summary>Parcela do Custo Incorrido sem direito ao crédito da atividade imobiliária, acumulado até o período.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlExcBcCusIncAcum { get; set; }

    /// <summary>Base de Cálculo do Crédito sobre o Custo Incorrido acumulado (Campo 04 – 05).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcCusInc { get; set; }

    /// <summary>Código da Situação Tributária referente ao PIS/PASEP, conforme Tabela 4.3.3.</summary>
    [CampoSped(Ordem = 7, Tamanho = 2, Obrigatorio = true)]
    public string? CstPis { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal AliqPis { get; set; }

    /// <summary>Valor Total do Crédito Acumulado sobre o custo incorrido – PIS/PASEP (Campo 06 x 08).</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredPisAcum { get; set; }

    /// <summary>Parcela do crédito descontada até o período anterior – PIS/PASEP.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredPisDescAnt { get; set; }

    /// <summary>Parcela a descontar no período da escrituração – PIS/PASEP.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredPisDesc { get; set; }

    /// <summary>Parcela a descontar em períodos futuros – PIS/PASEP (Campo 09 – 10 – 11).</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredPisDescFut { get; set; }

    /// <summary>Código da Situação Tributária referente à COFINS, conforme Tabela 4.3.4.</summary>
    [CampoSped(Ordem = 13, Tamanho = 2, Obrigatorio = true)]
    public string? CstCofins { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal AliqCofins { get; set; }

    /// <summary>Valor Total do Crédito Acumulado sobre o custo incorrido – COFINS (Campo 06 x 14).</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredCofinsAcum { get; set; }

    /// <summary>Parcela do crédito descontada até o período anterior – COFINS.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredCofinsDescAnt { get; set; }

    /// <summary>Parcela a descontar no período da escrituração – COFINS.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredCofinsDesc { get; set; }

    /// <summary>Parcela a descontar em períodos futuros – COFINS (Campo 15 – 16 – 17).</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredCofinsDescFut { get; set; }
}
