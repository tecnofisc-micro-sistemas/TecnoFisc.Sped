using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C170 — Itens do Documento (cód. 01, 1B, 04 e 55).
/// Nível hierárquico 3, ocorrência 1:N (por registro C100).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 76-77.
/// </summary>
/// <remarks>
/// <b>V016 (Guide 3.0.7 item 8):</b> validação adicional no campo 06 <c>UNID</c>.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </remarks>
[RegistroSped(Codigo = "C170", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC170 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C170";

    /// <summary>Número sequencial do item no documento fiscal.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public int NumItem { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Descrição complementar do item como adotado no documento fiscal.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? DescrCompl { get; set; }

    /// <summary>Quantidade do item.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 5, Obrigatorio = true)]
    public decimal? Qtd { get; set; }

    /// <summary>Unidade do item (campo 02 do Registro 0190).</summary>
    [CampoSped(Ordem = 6, Tamanho = 6, Obrigatorio = true)]
    public string? Unid { get; set; }

    /// <summary>Valor total do item (mercadorias ou serviços).</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    /// <summary>Valor do desconto comercial.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Movimentação física do item/produto: 0=SIM, 1=NÃO.</summary>
    [CampoSped(Ordem = 9, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentacaoFisica? IndMov { get; set; }

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme Tabela 4.3.1.</summary>
    [CampoSped(Ordem = 10, Tamanho = 3, Obrigatorio = true)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação.</summary>
    [CampoSped(Ordem = 11, Tamanho = 4, Obrigatorio = true)]
    public Cfop Cfop { get; set; }

    /// <summary>Código da natureza da operação (campo 02 do Registro 0400).</summary>
    [CampoSped(Ordem = 12, Tamanho = 10)]
    public string? CodNat { get; set; }

    /// <summary>Valor da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Alíquota do ICMS.</summary>
    [CampoSped(Ordem = 14, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Valor do ICMS creditado/debitado.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Valor da base de cálculo referente à substituição tributária.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcmsSt { get; set; }

    /// <summary>Alíquota do ICMS da substituição tributária na UF de destino.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2)]
    public decimal? AliqSt { get; set; }

    /// <summary>Valor do ICMS referente à substituição tributária.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcmsSt { get; set; }

    /// <summary>Indicador de período de apuração do IPI: 0=Mensal, 1=Decendial.</summary>
    [CampoSped(Ordem = 19, Tamanho = 1)]
    public IndicadorApuracaoIpi? IndApur { get; set; }

    /// <summary>Código da Situação Tributária referente ao IPI, conforme Tabela 4.3.2.</summary>
    [CampoSped(Ordem = 20, Tamanho = 2)]
    public string? CstIpi { get; set; }

    /// <summary>Código de enquadramento legal do IPI, conforme Tabela 4.5.3.</summary>
    [CampoSped(Ordem = 21, Tamanho = 3)]
    public string? CodEnq { get; set; }

    /// <summary>Valor da base de cálculo do IPI.</summary>
    [CampoSped(Ordem = 22, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIpi { get; set; }

    /// <summary>Alíquota do IPI.</summary>
    [CampoSped(Ordem = 23, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIpi { get; set; }

    /// <summary>Valor do IPI creditado/debitado.</summary>
    [CampoSped(Ordem = 24, Tamanho = 0, Decimais = 2)]
    public decimal? VlIpi { get; set; }

    /// <summary>Código da Situação Tributária referente ao PIS.</summary>
    [CampoSped(Ordem = 25, Tamanho = 2)]
    public int? CstPis { get; set; }

    /// <summary>Valor da base de cálculo do PIS.</summary>
    [CampoSped(Ordem = 26, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPis { get; set; }

    /// <summary>Alíquota do PIS (em percentual).</summary>
    [CampoSped(Ordem = 27, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPis { get; set; }

    /// <summary>Quantidade — base de cálculo do PIS.</summary>
    [CampoSped(Ordem = 28, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcPis { get; set; }

    /// <summary>Alíquota do PIS (em reais).</summary>
    [CampoSped(Ordem = 29, Tamanho = 0, Decimais = 4)]
    public decimal? AliqPisQuant { get; set; }

    /// <summary>Valor do PIS.</summary>
    [CampoSped(Ordem = 30, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Código da Situação Tributária referente ao COFINS.</summary>
    [CampoSped(Ordem = 31, Tamanho = 2)]
    public int? CstCofins { get; set; }

    /// <summary>Valor da base de cálculo do COFINS.</summary>
    [CampoSped(Ordem = 32, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    /// <summary>Alíquota do COFINS (em percentual).</summary>
    [CampoSped(Ordem = 33, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    /// <summary>Quantidade — base de cálculo do COFINS.</summary>
    [CampoSped(Ordem = 34, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcCofins { get; set; }

    /// <summary>Alíquota do COFINS (em reais).</summary>
    [CampoSped(Ordem = 35, Tamanho = 0, Decimais = 4)]
    public decimal? AliqCofinsQuant { get; set; }

    /// <summary>Valor do COFINS.</summary>
    [CampoSped(Ordem = 36, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 37, Tamanho = 0)]
    public string? CodCta { get; set; }

    /// <summary>Valor do abatimento não tributado e não comercial.</summary>
    [CampoSped(Ordem = 38, Tamanho = 0, Decimais = 2)]
    public decimal? VlAbatNt { get; set; }
}
