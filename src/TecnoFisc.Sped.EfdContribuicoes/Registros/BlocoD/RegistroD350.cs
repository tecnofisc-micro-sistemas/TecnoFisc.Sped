using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D350 — Resumo Diário de Cupom Fiscal Emitido por ECF (2E, 13, 14, 15, 16).
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 211.
/// </summary>
[RegistroSped(Codigo = "D350", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD350 : RegistroSped
{
    public override string Codigo => "D350";

    /// <summary>Código do modelo do documento fiscal conforme Tabela 4.1.1 (2E, 13, 14, 15, 16).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 20, Obrigatorio = true)]
    public string? EcfMod { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 21, Obrigatorio = true)]
    public string? EcfFab { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 3, Obrigatorio = true)]
    public int Cro { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 6, Obrigatorio = true)]
    public int Crz { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 6, Obrigatorio = true)]
    public int NumCooFin { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal GtFin { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBrt { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 2, Obrigatorio = true)]
    public int CstPis { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcPis { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPis { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcPis { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 4)]
    public decimal? AliqPisQuant { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 2, Obrigatorio = true)]
    public int CstCofins { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcCofins { get; set; }

    [CampoSped(Ordem = 19, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 3)]
    public decimal? QuantBcCofins { get; set; }

    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 4)]
    public decimal? AliqCofinsQuant { get; set; }

    [CampoSped(Ordem = 22, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    [CampoSped(Ordem = 23, Tamanho = 255)]
    public string? CodCta { get; set; }
}
