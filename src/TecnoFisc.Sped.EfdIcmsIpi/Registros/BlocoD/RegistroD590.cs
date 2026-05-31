using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D590 — Registro Analítico do Documento (código 21 e 22).
/// Totaliza por combinação de CST_ICMS, CFOP e alíquota do ICMS os documentos
/// de serviço de comunicação e telecomunicação do D500.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, pp. 196-197.
/// </summary>
[RegistroSped(Codigo = "D590", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD590 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D590";

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme a tabela indicada no item 4.3.1.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação, conforme a tabela indicada no item 4.2.2.</summary>
    [CampoSped(Ordem = 3, Tamanho = 4)]
    public Cfop? Cfop { get; set; }

    /// <summary>Alíquota do ICMS.</summary>
    [CampoSped(Ordem = 4, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Valor da operação correspondente à combinação de CST_ICMS, CFOP e alíquota do ICMS, incluídas as despesas acessórias e acréscimos.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlOpr { get; set; }

    /// <summary>Parcela correspondente ao valor da base de cálculo do ICMS referente à combinação CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Parcela correspondente ao valor do ICMS referente à combinação CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    /// <summary>Parcela correspondente ao valor da base de cálculo do ICMS de outras UFs, referente à combinação CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcmsUf { get; set; }

    /// <summary>Parcela correspondente ao valor do ICMS de outras UFs, referente à combinação CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcmsUf { get; set; }

    /// <summary>Valor não tributado em função da redução da base de cálculo do ICMS, referente à combinação CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlRedBc { get; set; }

    /// <summary>Código da observação do lançamento fiscal (campo 02 do Registro 0460).</summary>
    [CampoSped(Ordem = 11, Tamanho = 6)]
    public string? CodObs { get; set; }
}
