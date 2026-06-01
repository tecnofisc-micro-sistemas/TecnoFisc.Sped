using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C590 — Registro Analítico do Documento: NF/Conta de Energia Elétrica (cód. 06),
/// NF de Energia Elétrica Eletrônica – NF3e (cód. 66), NF/Conta de Fornecimento D'Água
/// Canalizada (cód. 29) e NF Fiscal Consumo Fornecimento de Gás (cód. 28).
/// Totaliza os itens do C510 por combinação de CST_ICMS, CFOP e alíquota do ICMS.
/// Nível hierárquico 3, ocorrência 1:N (por registro C500).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, pp. 139-140.
/// </summary>
/// <remarks>
/// <b>V016 (Guide 3.0.7 item 14):</b> nova orientação de preenchimento para o campo 05
/// <c>VL_OPR</c> referente à entrega de NF3-e (modelo 66).
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// <para>
/// <b>V017 (Guide 3.1.4 item 3):</b> nova orientação determinando que NF3-e (modelo 66)
/// sem CST e sem energia injetada não deve ser escriturada neste registro analítico.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </para>
/// </remarks>
[RegistroSped(Codigo = "C590", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC590 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C590";

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme Tabela indicada no item 4.3.1.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação do agrupamento de itens.</summary>
    [CampoSped(Ordem = 3, Tamanho = 4, Obrigatorio = true)]
    public Cfop Cfop { get; set; }

    /// <summary>Alíquota do ICMS.</summary>
    [CampoSped(Ordem = 4, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Valor da operação correspondente à combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOpr { get; set; }

    /// <summary>Parcela correspondente ao valor da base de cálculo do ICMS referente à combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Parcela correspondente ao valor do ICMS referente à combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Parcela correspondente ao valor da base de cálculo do ICMS da substituição tributária referente à combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcmsSt { get; set; }

    /// <summary>Parcela correspondente ao valor creditado/debitado do ICMS da substituição tributária referente à combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcmsSt { get; set; }

    /// <summary>Valor não tributado em função da redução da base de cálculo do ICMS referente à combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlRedBc { get; set; }

    /// <summary>Código da observação do lançamento fiscal (campo 02 do Registro 0460).</summary>
    [CampoSped(Ordem = 11, Tamanho = 6)]
    public string? CodObs { get; set; }
}
