using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D300 — Resumo da Escrituração Diária – Bilhetes Consolidados de Passagem (13, 14, 15, 16, 18).
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 208.
/// </summary>
[RegistroSped(Codigo = "D300", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD300 : RegistroSped
{
    public override string Codigo => "D300";

    /// <summary>Código do modelo do documento fiscal conforme Tabela 4.1.1 (13, 14, 15, 16, 18).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 4)]
    public string? Ser { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 3)]
    public int? Sub { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 6)]
    public long? NumDocIni { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 6)]
    public long? NumDocFin { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 4, Obrigatorio = true)]
    public Cfop Cfop { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtRef { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 2, Obrigatorio = true)]
    public int CstPis { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPis { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPis { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 2, Obrigatorio = true)]
    public int CstCofins { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    [CampoSped(Ordem = 19, Tamanho = 255)]
    public string? CodCta { get; set; }
}
