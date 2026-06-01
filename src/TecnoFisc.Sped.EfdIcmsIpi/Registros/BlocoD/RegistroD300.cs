using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D300 — Registro Analítico dos Bilhetes Consolidados de Passagem Rodoviário (13),
/// Aquaviário (14), Passagem e Bagagem (15) e Ferroviário (16).
/// Consolida por combinação de CST_ICMS, CFOP e alíquota todos os bilhetes de passagem
/// do mesmo modelo, série, subsérie e data, conforme legislação estadual.
/// Nível hierárquico 2, ocorrência vários por arquivo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 181.
/// </summary>
[RegistroSped(Codigo = "D300", Nivel = 2, Bloco = "D")]
public sealed partial class RegistroD300 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D300";

    /// <summary>Código do modelo do documento fiscal, conforme Tabela 4.1.1 (valores válidos: 13, 14, 15, 16).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 4, Obrigatorio = true)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal.</summary>
    [CampoSped(Ordem = 4, Tamanho = 4)]
    public int? Sub { get; set; }

    /// <summary>Número do primeiro documento fiscal emitido (mesmo modelo, série e subsérie).</summary>
    [CampoSped(Ordem = 5, Tamanho = 6, Obrigatorio = true)]
    public int? NumDocIni { get; set; }

    /// <summary>Número do último documento fiscal emitido (mesmo modelo, série e subsérie).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Obrigatorio = true)]
    public int? NumDocFin { get; set; }

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme a tabela indicada no item 4.3.1.</summary>
    [CampoSped(Ordem = 7, Tamanho = 3, Obrigatorio = true)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação, conforme a tabela indicada no item 4.2.2.</summary>
    [CampoSped(Ordem = 8, Tamanho = 4, Obrigatorio = true)]
    public Cfop? Cfop { get; set; }

    /// <summary>Alíquota do ICMS.</summary>
    [CampoSped(Ordem = 9, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Data da emissão dos documentos fiscais no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 10, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Valor total acumulado das operações correspondentes à combinação de CST_ICMS, CFOP e alíquota do ICMS, incluídas as despesas acessórias e acréscimos.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOpr { get; set; }

    /// <summary>Valor total dos descontos.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Valor total da prestação de serviço.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlServ { get; set; }

    /// <summary>Valor de seguro.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlSeg { get; set; }

    /// <summary>Valor de outras despesas.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlOutDesp { get; set; }

    /// <summary>Valor total da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIcms { get; set; }

    /// <summary>Valor total do ICMS.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcms { get; set; }

    /// <summary>Valor não tributado em função da redução da base de cálculo do ICMS, referente à combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRedBc { get; set; }

    /// <summary>Código da observação do lançamento fiscal (campo 02 do Registro 0460).</summary>
    [CampoSped(Ordem = 19, Tamanho = 6)]
    public string? CodObs { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 20, Tamanho = 0)]
    public string? CodCta { get; set; }
}
