using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C600 — Consolidação Diária de NF/Conta de Energia Elétrica (06), NF3e (66),
/// Água (29), Gás (28) e NF-e (55) – Documentos de Saída.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 172.
/// </summary>
[RegistroSped(Codigo = "C600", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC600 : RegistroSped
{
    public override string Codigo => "C600";

    /// <summary>Código do modelo do documento fiscal — Tabela 4.1.1. Valores válidos: 01, 06, 28, 29, 55, 66.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código do município dos pontos de consumo, conforme tabela IBGE (7 dígitos).</summary>
    [CampoSped(Ordem = 3, Tamanho = 7)]
    public int? CodMun { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 4)]
    public string? Ser { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 3)]
    public int? Sub { get; set; }

    /// <summary>Código de classe de consumo de energia elétrica (Tabela 4.4.5), água (Tabela 4.4.2) ou gás (Tabela 4.4.3).</summary>
    [CampoSped(Ordem = 6, Tamanho = 2)]
    public int? CodCons { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Obrigatorio = true)]
    public long QtdCons { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0)]
    public long? QtdCanc { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Consumo total acumulado, em kWh (apenas Código 06).</summary>
    [CampoSped(Ordem = 12, Tamanho = 0)]
    public long? Cons { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlForn { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlServNt { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlTerc { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlDa { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcmsSt { get; set; }

    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcmsSt { get; set; }

    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlPis { get; set; }

    [CampoSped(Ordem = 22, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCofins { get; set; }
}
