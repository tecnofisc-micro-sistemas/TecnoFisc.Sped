using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D200 — Resumo Diário – Prestação de Serviços de Transporte (07, 08, 8B, 09, 10,
/// 11, 26, 27, 57, 63, 67). Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 203.
/// </summary>
[RegistroSped(Codigo = "D200", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD200 : RegistroSped
{
    public override string Codigo => "D200";

    /// <summary>Código do modelo do documento fiscal conforme Tabela 4.1.1 (07, 08, 8B, 09, 10, 11, 26, 27, 57, 63, 67).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código da situação do documento fiscal conforme Tabela 4.1.2 (00, 01, 06, 07, 08).</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal CodSit { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 4)]
    public string? Ser { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 3)]
    public string? Sub { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 9, Obrigatorio = true)]
    public long NumDocIni { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 9, Obrigatorio = true)]
    public long NumDocFin { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 4, Obrigatorio = true)]
    public Cfop Cfop { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtRef { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }
}
