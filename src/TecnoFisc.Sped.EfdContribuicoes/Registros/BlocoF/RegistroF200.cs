using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F200 — Operações da Atividade Imobiliária - Unidade Imobiliária Vendida.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 249.
/// </summary>
[RegistroSped(Codigo = "F200", Nivel = 3, Bloco = "F")]
public sealed partial class RegistroF200 : RegistroSped
{
    public override string Codigo => "F200";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public IndicadorOperacaoImobiliaria IndOper { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public IndicadorUnidadeImobiliaria UnidImob { get; set; }

    /// <summary>Identificação/Nome do Empreendimento.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true)]
    public string? IdentEmp { get; set; }

    /// <summary>Descrição resumida da unidade imobiliária vendida.</summary>
    [CampoSped(Ordem = 5, Tamanho = 90)]
    public string? DescUnidImob { get; set; }

    /// <summary>Número do Contrato/Documento que formaliza a Venda da Unidade Imobiliária.</summary>
    [CampoSped(Ordem = 6, Tamanho = 90)]
    public string? NumCont { get; set; }

    /// <summary>CPF (11 dígitos) ou CNPJ (14 dígitos) do adquirente da unidade imobiliária.</summary>
    [CampoSped(Ordem = 7, Tamanho = 14, Obrigatorio = true)]
    public string? CpfCnpjAdq { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtOper { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotVend { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlRecAcum { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotRec { get; set; }

    /// <summary>Código da Situação Tributária referente ao PIS/PASEP, conforme Tabela 4.3.3.</summary>
    [CampoSped(Ordem = 12, Tamanho = 2, Obrigatorio = true)]
    public string? CstPis { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPis { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPis { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Código da Situação Tributária referente à COFINS, conforme Tabela 4.3.4.</summary>
    [CampoSped(Ordem = 16, Tamanho = 2, Obrigatorio = true)]
    public string? CstCofins { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>
    /// Percentual da receita total recebida até o mês: (VL_REC_ACUM + VL_TOT_REC) / VL_TOT_VEND.
    /// </summary>
    [CampoSped(Ordem = 20, Tamanho = 6, Decimais = 2)]
    public decimal? PercRecReceb { get; set; }

    [CampoSped(Ordem = 21, Tamanho = 1)]
    public IndicadorNaturezaEmpreendimento? IndNatEmp { get; set; }

    /// <summary>Informações Complementares.</summary>
    [CampoSped(Ordem = 22, Tamanho = 90)]
    public string? InfComp { get; set; }
}
