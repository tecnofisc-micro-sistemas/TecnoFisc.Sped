using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoP;

/// <summary>
/// Registro P210 — Ajuste da Contribuição Previdenciária Apurada Sobre a Receita Bruta.
/// Nível hierárquico 3, ocorrência 1:N (filho de P200). Detalha os ajustes de redução
/// (IND_AJ = 0) e acréscimo (IND_AJ = 1) sobre a CPRB apurada. Conforme Guia Prático v1.35, p. 373.
/// </summary>
[RegistroSped(Codigo = "P210", Nivel = 3, Bloco = "P")]
public sealed partial class RegistroP210 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "P210";

    /// <summary>Indicador do tipo de ajuste: 0 redução, 1 acréscimo.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorAjuste IndAj { get; set; }

    /// <summary>Valor do ajuste.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAj { get; set; }

    /// <summary>
    /// Código do ajuste conforme Tabela 4.3.8 — Tabela Código de Ajustes de Contribuição
    /// ou Créditos (2 caracteres fixos).
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true)]
    public string? CodAj { get; set; }

    /// <summary>Número do processo, documento ou ato concessório ao qual o ajuste está vinculado.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? NumDoc { get; set; }

    /// <summary>Descrição resumida do ajuste.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? DescrAj { get; set; }

    /// <summary>Data de referência do ajuste no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtRef { get; set; }
}
