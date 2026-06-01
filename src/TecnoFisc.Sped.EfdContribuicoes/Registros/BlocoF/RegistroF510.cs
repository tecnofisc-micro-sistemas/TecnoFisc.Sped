using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F510 — Consolidação das Operações da Pessoa Jurídica Submetida ao Regime de Tributação
/// com Base no Lucro Presumido – Incidência do PIS/Pasep e da Cofins pelo Regime de Caixa
/// (Apuração da Contribuição por Unidade de Medida de Produto – Alíquota em Reais).
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 262.
/// </summary>
[RegistroSped(Codigo = "F510", Nivel = 3, Bloco = "F")]
public sealed partial class RegistroF510 : RegistroSped
{
    public override string Codigo => "F510";

    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRecCaixa { get; set; }

    /// <summary>Código da Situação Tributária referente ao PIS/PASEP, conforme Tabela II IN RFB 1.009/2010.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? CstPis { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlDescPis { get; set; }

    /// <summary>Base de cálculo do PIS/PASEP em quantidade (Unidade de Medida de Produto).</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcPis { get; set; }

    /// <summary>Alíquota do PIS/PASEP em reais.</summary>
    [CampoSped(Ordem = 6, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPisQuant { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Código da Situação Tributária referente à COFINS, conforme Tabela III IN RFB 1.009/2010.</summary>
    [CampoSped(Ordem = 8, Tamanho = 2, Obrigatorio = true)]
    public string? CstCofins { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? VlDescCofins { get; set; }

    /// <summary>Base de cálculo da Cofins em quantidade (Unidade de Medida de Produto).</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcCofins { get; set; }

    /// <summary>Alíquota da Cofins em reais.</summary>
    [CampoSped(Ordem = 11, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofinsQuant { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Código do modelo do documento fiscal conforme Tabela 4.1.1.</summary>
    [CampoSped(Ordem = 13, Tamanho = 2)]
    public string? CodMod { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 4)]
    public Cfop? Cfop { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 15, Tamanho = 255)]
    public string? CodCta { get; set; }

    /// <summary>Informação complementar.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0)]
    public string? InfoCompl { get; set; }
}
