using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D410 — Documentos Informados (códigos 13, 14, 15 e 16).
/// Informa os bilhetes de passagem (Rodoviário 13, Aquaviário 14, Passagem/Bagagem 15, Ferroviário 16)
/// consolidados no RMD do registro D400, não emitidos por ECF.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 189.
/// </summary>
[RegistroSped(Codigo = "D410", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD410 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D410";

    /// <summary>Código do modelo do documento fiscal conforme Tabela 4.1.1 (valores válidos: 13, 14, 15, 16).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 4, Obrigatorio = true)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3)]
    public int? Sub { get; set; }

    /// <summary>Número do documento fiscal inicial (mesmo modelo, série e subsérie).</summary>
    [CampoSped(Ordem = 5, Tamanho = 6, Obrigatorio = true)]
    public int? NumDocIni { get; set; }

    /// <summary>Número do documento fiscal final (mesmo modelo, série e subsérie).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Obrigatorio = true)]
    public int? NumDocFin { get; set; }

    /// <summary>Data da emissão dos documentos fiscais no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Código da Situação Tributária referente ao ICMS conforme Tabela 4.3.1 (3 dígitos, 1º sempre 0).</summary>
    [CampoSped(Ordem = 8, Tamanho = 3, Obrigatorio = true)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação conforme Convênio SN/70 (1º caractere deve ser 5 ou 6).</summary>
    [CampoSped(Ordem = 9, Tamanho = 4, Obrigatorio = true)]
    public Cfop? Cfop { get; set; }

    /// <summary>Alíquota do ICMS.</summary>
    [CampoSped(Ordem = 10, Tamanho = 6, Decimais = 2)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Valor total acumulado das operações correspondentes à combinação de CST_ICMS, CFOP e alíquota do ICMS, incluídas as despesas acessórias e acréscimos.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOpr { get; set; }

    /// <summary>Valor acumulado dos descontos.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Valor acumulado da prestação de serviço.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlServ { get; set; }

    /// <summary>Valor acumulado da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIcms { get; set; }

    /// <summary>Valor acumulado do ICMS.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcms { get; set; }
}
