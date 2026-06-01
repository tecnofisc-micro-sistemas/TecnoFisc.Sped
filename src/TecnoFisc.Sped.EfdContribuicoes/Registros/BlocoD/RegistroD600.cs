using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D600 — Consolidação da Prestação de Serviços — Notas de Serviço de Comunicação
/// (Código 21) e de Serviço de Telecomunicação (Código 22).
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 221.
/// </summary>
[RegistroSped(Codigo = "D600", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD600 : RegistroSped
{
    public override string Codigo => "D600";

    /// <summary>Código do modelo do documento fiscal, conforme Tabela 4.1.1. Valores válidos: 21, 22.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código do município dos terminais faturados, conforme tabela IBGE (7 dígitos).</summary>
    [CampoSped(Ordem = 3, Tamanho = 7)]
    public string? CodMun { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 4)]
    public string? Ser { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 3)]
    public string? Sub { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoReceitaTelecom IndRec { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Obrigatorio = true)]
    public int QtdCons { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDocIni { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDocFin { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlServ { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2)]
    public decimal? VlServNt { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2)]
    public decimal? VlTerc { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlDa { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }
}
