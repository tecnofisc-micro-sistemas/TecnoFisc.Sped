using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D390 — Registro Analítico do Movimento Diário (cód. 13, 14, 15, 16 e 2E).
/// Escritura os documentos fiscais emitidos por ECF totalizados pela combinação de CST_ICMS, CFOP e Alíquota.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 194.
/// </summary>
[RegistroSped(Codigo = "D390", Nivel = 4, Bloco = "D")]
public sealed partial class RegistroD390 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D390";

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme Tabela indicada no item 4.3.1.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação.</summary>
    [CampoSped(Ordem = 3, Tamanho = 4, Obrigatorio = true)]
    public Cfop? Cfop { get; set; }

    /// <summary>Alíquota do ICMS.</summary>
    [CampoSped(Ordem = 4, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Valor da operação correspondente à combinação de CST_ICMS, CFOP e alíquota do ICMS, incluídas as despesas acessórias e acréscimos.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlOpr { get; set; }

    /// <summary>Valor da base de cálculo do ISSQN.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIssqn { get; set; }

    /// <summary>Alíquota do ISSQN.</summary>
    [CampoSped(Ordem = 7, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIssqn { get; set; }

    /// <summary>Valor do ISSQN.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlIssqn { get; set; }

    /// <summary>Base de cálculo do ICMS acumulada relativa à alíquota informada.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor do ICMS acumulado relativo à alíquota informada.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlIcms { get; set; }

    /// <summary>Código da observação do lançamento fiscal (campo 02 do Registro 0460).</summary>
    [CampoSped(Ordem = 11, Tamanho = 6)]
    public string? CodObs { get; set; }
}
