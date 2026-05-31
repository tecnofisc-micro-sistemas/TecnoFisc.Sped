using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M505 — Detalhamento da Base de Cálculo do Crédito Apurado no Período – Cofins.
/// Nível hierárquico 3, ocorrência 1:N (filho de M500).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 332.
/// </summary>
[RegistroSped(Codigo = "M505", Nivel = 3, Bloco = "M")]
public sealed partial class RegistroM505 : RegistroSped
{
    public override string Codigo => "M505";

    /// <summary>Código da Natureza da Base de Cálculo do Crédito, conforme Tabela 4.3.7.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public CodigoBaseCalculoCredito NatBcCred { get; set; }

    /// <summary>Código da Situação Tributária referente ao crédito de Cofins (Tabela 4.3.4), vinculado ao tipo de crédito escriturado em M500.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public int CstCofins { get; set; }

    /// <summary>Valor Total da Base de Cálculo escriturada nos documentos e operações (Blocos A, C, D e F), referente ao CST_COFINS informado.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofinsTot { get; set; }

    /// <summary>Parcela do Valor Total da Base de Cálculo vinculada a receitas com incidência cumulativa.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofinsCum { get; set; }

    /// <summary>Valor Total da Base de Cálculo do Crédito vinculada a receitas com incidência não-cumulativa (Campo 04 – Campo 05).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofinsNc { get; set; }

    /// <summary>Valor da Base de Cálculo do Crédito vinculada ao tipo de crédito escriturado em M500.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    /// <summary>Quantidade Total da Base de Cálculo do Crédito apurada em Unidade de Medida de Produto, escriturada nos documentos e operações (Blocos A, C, D e F), referente ao CST_COFINS informado.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcCofinsTot { get; set; }

    /// <summary>Parcela da base de cálculo do crédito em quantidade vinculada ao tipo de crédito escriturado em M500.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcCofins { get; set; }

    /// <summary>Descrição do crédito. Obrigatório quando NAT_BC_CRED = 13 (Outras Operações com Direito a Crédito).</summary>
    [CampoSped(Ordem = 10, Tamanho = 60)]
    public string? DescCred { get; set; }
}
