using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M110 — Ajustes do Crédito de PIS/Pasep Apurado.
/// Nível hierárquico 3, ocorrência 1:N (filho de M100).
/// Conforme Guia Prático EFD Contribuições v1.35, p. 303.
/// </summary>
[RegistroSped(Codigo = "M110", Nivel = 3, Bloco = "M")]
public sealed partial class RegistroM110 : RegistroSped
{
    public override string Codigo => "M110";

    /// <summary>Indicador do tipo de ajuste: 0=Redução; 1=Acréscimo.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorAjuste IndAj { get; set; }

    /// <summary>Valor do ajuste.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAj { get; set; }

    /// <summary>Código do ajuste, conforme Tabela 4.3.8.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true)]
    public string? CodAj { get; set; }

    /// <summary>Número do processo, documento ou ato concessório ao qual o ajuste está vinculado.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? NumDoc { get; set; }

    /// <summary>Descrição resumida do ajuste.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? DescrAj { get; set; }

    /// <summary>Data de referência do ajuste (ddmmaaaa).</summary>
    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtRef { get; set; }
}
