using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C600 — Consolidação Diária de NF/Contas de Energia Elétrica (cód. 06), NF/Conta de
/// Fornecimento D'Água Canalizada (cód. 29) e NF/Conta de Fornecimento de Gás Canalizado (cód. 28)
/// para empresas não obrigadas ao Convênio ICMS 115/2003.
/// Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, pp. 143-144.
/// </summary>
[RegistroSped(Codigo = "C600", Nivel = 2, Bloco = "C")]
public sealed partial class RegistroC600 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C600";

    /// <summary>Código do modelo do documento fiscal, conforme Tabela 4.1.1. Valores válidos: 06, 28, 29.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código do município dos pontos de consumo, conforme tabela IBGE (7 dígitos).</summary>
    [CampoSped(Ordem = 3, Tamanho = 7, Obrigatorio = true)]
    public int? CodMun { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 4, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal.</summary>
    [CampoSped(Ordem = 5, Tamanho = 3)]
    public int? Sub { get; set; }

    /// <summary>
    /// Código de classe de consumo de energia elétrica ou gás (modelos 06 e 28): 01-Comercial;
    /// 02-Consumo Próprio; 03-Iluminação Pública; 04-Industrial; 05-Poder Público;
    /// 06-Residencial; 07-Rural; 08-Serviço Público. Para água canalizada (29): conforme Tabela 4.4.2.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Obrigatorio = true)]
    public string? CodCons { get; set; }

    /// <summary>Quantidade de documentos consolidados neste registro. Deve ser maior que zero.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Obrigatorio = true)]
    public int? QtdCons { get; set; }

    /// <summary>Quantidade de documentos cancelados.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0)]
    public int? QtdCanc { get; set; }

    /// <summary>Data dos documentos consolidados, no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 9, Tamanho = 8, Obrigatorio = true, Formato = "ddMMyyyy")]
    public DateOnly? DtDoc { get; set; }

    /// <summary>Valor total dos documentos.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlDoc { get; set; }

    /// <summary>Valor acumulado dos descontos.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Consumo total acumulado em kWh. Aplicável somente ao código 06 (energia elétrica).</summary>
    [CampoSped(Ordem = 12, Tamanho = 0)]
    public long? Cons { get; set; }

    /// <summary>Valor acumulado do fornecimento.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlForn { get; set; }

    /// <summary>Valor acumulado dos serviços não-tributados pelo ICMS.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlServNt { get; set; }

    /// <summary>Valores cobrados em nome de terceiros.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlTerc { get; set; }

    /// <summary>Valor acumulado das despesas acessórias.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlDa { get; set; }

    /// <summary>Valor acumulado da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor acumulado do ICMS.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Valor acumulado da base de cálculo do ICMS substituição tributária.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcmsSt { get; set; }

    /// <summary>Valor acumulado do ICMS retido por substituição tributária.</summary>
    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcmsSt { get; set; }

    /// <summary>Valor acumulado do PIS. Dispensado quando o declarante entrega EFD-Contribuições relativa ao mesmo período.</summary>
    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor acumulado da COFINS. Dispensado quando o declarante entrega EFD-Contribuições relativa ao mesmo período.</summary>
    [CampoSped(Ordem = 22, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }
}
