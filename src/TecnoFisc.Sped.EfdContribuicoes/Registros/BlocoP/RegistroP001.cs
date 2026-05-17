using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoP;

/// <summary>
/// Registro P001 — Abertura do Bloco P (apuração da Contribuição Previdenciária Sobre a
/// Receita Bruta — CPRB). Nível hierárquico 1, ocorrência única por arquivo. Conforme
/// Guia Prático v1.35, p. 366.
/// </summary>
[RegistroSped(Codigo = "P001", Nivel = 1, Bloco = "P")]
public sealed partial class RegistroP001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "P001";

    /// <summary>Indicador de movimento do bloco — 0 com dados, 1 sem dados.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndMov { get; set; }
}
