using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1800 — Incorporação Imobiliária – RET.
/// Nível hierárquico 2, ocorrência vários (por arquivo). Escriturado pela pessoa jurídica que executa
/// empreendimentos de incorporação imobiliária apurando contribuição social com base em Regime Especial
/// de Tributação – RET (IN RFB nº 1.435/2013).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 408.
/// </summary>
[RegistroSped(Codigo = "1800", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1800 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1800";

    /// <summary>Empreendimento objeto de incorporação imobiliária, optante pelo RET (CNPJ do empreendimento, 14 dígitos sem separadores).</summary>
    [CampoSped(Ordem = 2, Tamanho = 90, Obrigatorio = true)]
    public string? IncImob { get; set; }

    /// <summary>Receitas recebidas pela incorporadora na venda das unidades imobiliárias que compõem a incorporação.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal RecRecebRet { get; set; }

    /// <summary>Receitas financeiras e variações monetárias decorrentes das vendas submetidas ao RET.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? RecFinRet { get; set; }

    /// <summary>Base de cálculo do recolhimento unificado.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal BcRet { get; set; }

    /// <summary>Alíquota do recolhimento unificado.</summary>
    [CampoSped(Ordem = 6, Tamanho = 6, Decimais = 2, Obrigatorio = true)]
    public decimal AliqRet { get; set; }

    /// <summary>Valor do recolhimento unificado.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRecUni { get; set; }

    /// <summary>Data do recolhimento unificado no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 8, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtRecUni { get; set; }

    /// <summary>Código da receita conforme DARF ou DCOMP.</summary>
    [CampoSped(Ordem = 9, Tamanho = 4)]
    public string? CodRec { get; set; }
}
