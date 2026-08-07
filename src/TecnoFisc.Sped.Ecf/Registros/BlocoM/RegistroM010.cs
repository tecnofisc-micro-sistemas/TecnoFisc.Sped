using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoM;

/// <summary>Registro M010 - conta da Parte B do e-Lalur e do e-Lacs.</summary>
[RegistroSped(Codigo = "M010", Nivel = 2, Bloco = "M")]
public sealed partial class RegistroM010 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M010";

    /// <summary>Código Unívoco Atribuídopela Pessoa Jurídica à Conta no e-Lalur</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "COD_CTA_B")]
    public string? CodCtaB { get; set; }

    /// <summary>Descrição da Conta.</summary>
    [CampoSped(Ordem = 3, Obrigatorio = true, Nome = "DESC_CTA_LAL")]
    public string? DescCtaLal { get; set; }

    /// <summary>Data final do período de apuração em que a conta foi criada.</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "DT_AP_LAL")]
    public DateOnly DtApLal { get; set; }

    /// <summary>Código da tabela padrão da Parte B. Vide planilha PARTEB_PADRAO do arquivo “Tabelas_Dinamicas_ECF_Leiaute_11_AC2024_SIT_ESP_2025.xlsx” a partir do link http://sped.rfb.gov.br/pasta/show/1644. Os relacionamentos possíveis das contas padrão da Parte B do e-Lalur e do e-Lacs com as linhas da Parte A constam da planilha PARTEB_PARTEA do mesmo arquivo.</summary>
    [CampoSped(Ordem = 5, Tamanho = 6, Obrigatorio = true, Nome = "COD_PB_RFB")]
    public string? CodPbRfb { get; set; }

    /// <summary>Data limite para exclusão, adição ou compensação do valor controlado.</summary>
    [CampoSped(Ordem = 6, Tamanho = 8, Formato = "ddMMyyyy", Nome = "DT_LIM_LAL")]
    public DateOnly? DtLimLal { get; set; }

    /// <summary>Indicador do Tributo da Adição/Exclusão: I – Imposto de Renda Pessoa Jurídica C – Contribuição Social sobre o Lucro Líquido</summary>
    [CampoSped(Ordem = 7, Tamanho = 1, Obrigatorio = true, Nome = "COD_TRIBUTO")]
    public IndicadorTributoContaParteB CodTributo { get; set; }

    /// <summary>Saldo Inicial: Saldo no período inicial desta escrituração. Se M010.DT_AP_LAL for noperíodo da escrituração,então o valor deve ser zero</summary>
    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_SALDO_INI")]
    public decimal VlSaldoIni { get; set; }

    /// <summary>Indicador do Saldo Inicial: D – Para prejuízos ou valores que reduzam o lucro real ou a base de cálculo da contribuição social em períodos subsequentes. C – Para valores que aumentem o lucro real ou a base de cálculo da contribuição social em períodos subsequentes.</summary>
    [CampoSped(Ordem = 9, Tamanho = 1, Obrigatorio = true, Nome = "IND_VL_SALDO_INI")]
    public IndicadorDebitoCredito IndVlSaldoIni { get; set; }

    /// <summary>CNPJ da outra pessoa jurídica relacionada com evento originário da conta. Exemplos: 1- Identificar a investida no caso de valores (ganhos/perdas no novo AVJ) da participação societária anterior, nos caso de aquisições em estágios. 2- Identificar a investida no caso de amortização de mais-valia e menos-valia. 3- Identificar a investida no caso de impairment de goodwill, mais-valia e menos-valia. 4- Identificar a investida no caso de ganho por compra vantajosa. 5- Identificar a investida no caso registro do ágio gerado na aquisição de participação societária ocorrida até 31/12/2009. 6 - Identificar a investida no caso de ágio gerado pela sistemática de transição disciplinada no art. 65, Lei Nº 12.973/14. 7 - Identificar a pessoa jurídica antecessora no caso de conta incorporada devido a evento societário.</summary>
    [CampoSped(Ordem = 10, Tamanho = 14, Nome = "CNPJ_SIT_ESP")]
    public Cnpj? CnpjSitEsp { get; set; }
}
