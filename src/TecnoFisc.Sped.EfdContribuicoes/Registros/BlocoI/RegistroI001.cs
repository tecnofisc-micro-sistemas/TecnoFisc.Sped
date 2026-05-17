using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoI;

/// <summary>
/// Registro I001 — Abertura do Bloco I (operações de instituições financeiras, seguradoras
/// e assemelhados). Nível hierárquico 1, ocorrência única por arquivo. Conforme Guia
/// Prático v1.35, p. 284.
/// </summary>
[RegistroSped(Codigo = "I001", Nivel = 1, Bloco = "I")]
public sealed partial class RegistroI001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I001";

    /// <summary>Indicador de movimento do bloco — 0 com dados, 1 sem dados.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndMov { get; set; }
}
