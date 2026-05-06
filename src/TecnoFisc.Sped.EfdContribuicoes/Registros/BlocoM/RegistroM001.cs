using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M001 — Abertura do Bloco M (apuração da contribuição e crédito do PIS/Pasep e
/// Cofins). Nível hierárquico 1, ocorrência única por arquivo. Conforme Guia Prático v1.35,
/// p. 295.
/// </summary>
[RegistroSped(Codigo = "M001", Nivel = 1, Bloco = "M")]
public sealed partial class RegistroM001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M001";

    /// <summary>Indicador de movimento do bloco — 0 com dados, 1 sem dados.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndMov { get; set; }
}
