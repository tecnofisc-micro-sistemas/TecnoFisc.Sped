using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M105 — Detalhamento da Base de Cálculo do Crédito Apurado no Período – PIS/Pasep.
/// Nível hierárquico 3, ocorrência 1:N (filho de M100).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 299.
/// </summary>
[RegistroSped(Codigo = "M105", Nivel = 3, Bloco = "M")]
public sealed partial class RegistroM105 : RegistroSped
{
    public override string Codigo => "M105";

    /// <summary>Código da Natureza da Base de Cálculo do Crédito, conforme Tabela 4.3.7.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public CodigoBaseCalculoCredito NatBcCred { get; set; }

    /// <summary>Código da Situação Tributária referente ao crédito de PIS/Pasep (Tabela 4.3.3), vinculado ao tipo de crédito escriturado em M100.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public int CstPis { get; set; }

    /// <summary>Valor Total da Base de Cálculo escriturada nos documentos e operações (Blocos A, C, D e F), referente ao CST_PIS informado.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPisTot { get; set; }

    /// <summary>Parcela do Valor Total da Base de Cálculo vinculada a receitas com incidência cumulativa.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPisCum { get; set; }

    /// <summary>Valor Total da Base de Cálculo do Crédito vinculada a receitas com incidência não-cumulativa (Campo 04 – Campo 05).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPisNc { get; set; }

    /// <summary>Valor da Base de Cálculo do Crédito vinculada ao tipo de crédito escriturado em M100.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPis { get; set; }

    /// <summary>Quantidade Total da Base de Cálculo do Crédito apurada em Unidade de Medida de Produto, escriturada nos documentos e operações (Blocos A, C, D e F), referente ao CST_PIS informado.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcPisTot { get; set; }

    /// <summary>Parcela da base de cálculo do crédito em quantidade vinculada ao tipo de crédito escriturado em M100.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcPis { get; set; }

    /// <summary>Descrição do crédito. Obrigatório quando NAT_BC_CRED = 13 (Outras Operações com Direito a Crédito).</summary>
    [CampoSped(Ordem = 10, Tamanho = 60)]
    public string? DescCred { get; set; }
}
