using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F120 — Bens Incorporados ao Ativo Imobilizado – Operações Geradoras de Créditos com
/// Base nos Encargos de Depreciação e Amortização. Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 237.
/// </summary>
[RegistroSped(Codigo = "F120", Nivel = 3, Bloco = "F")]
public sealed partial class RegistroF120 : RegistroSped
{
    public override string Codigo => "F120";

    /// <summary>
    /// Código da Base de Cálculo do Crédito sobre Bens Incorporados ao Ativo Imobilizado,
    /// conforme Tabela 4.3.7. Valores válidos: 09 (Depreciação) ou 11 (Amortização).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? NatBcCred { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public IdentificadorBemImobilizado IdentBemImob { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 1)]
    public IndicadorOrigemCredito? IndOrigCred { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public IndicadorUtilizacaoBemImobilizado IndUtilBemImob { get; set; }

    /// <summary>Valor do Encargo de Depreciação/Amortização Incorrido no Período.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOperDep { get; set; }

    /// <summary>Parcela do Valor do Encargo de Depreciação/Amortização a excluir da base de cálculo do Crédito.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? ParcOperNaoBcCred { get; set; }

    /// <summary>Código da Situação Tributária referente ao PIS/PASEP, conforme Tabela 4.3.3.</summary>
    [CampoSped(Ordem = 8, Tamanho = 2, Obrigatorio = true)]
    public string? CstPis { get; set; }

    /// <summary>Base de cálculo do Crédito de PIS/PASEP no período (Campo 06 – Campo 07).</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPis { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPis { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Código da Situação Tributária referente à COFINS, conforme Tabela 4.3.4.</summary>
    [CampoSped(Ordem = 12, Tamanho = 2, Obrigatorio = true)]
    public string? CstCofins { get; set; }

    /// <summary>Base de Cálculo do Crédito da COFINS no período (Campo 06 – Campo 07).</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 16, Tamanho = 255)]
    public string? CodCta { get; set; }

    /// <summary>Código do Centro de Custos.</summary>
    [CampoSped(Ordem = 17, Tamanho = 255)]
    public string? CodCcus { get; set; }

    /// <summary>Descrição complementar do bem ou grupo de bens.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0)]
    public string? DescBemImob { get; set; }
}
