using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D001 — Abertura do Bloco D (documentos fiscais II — serviços / ICMS).
/// Nível hierárquico 1, ocorrência única por arquivo. Conforme Guia Prático v1.35, p. 193.
/// </summary>
[RegistroSped(Codigo = "D001", Nivel = 1, Bloco = "D")]
public sealed partial class RegistroD001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D001";

    /// <summary>Indicador de movimento do bloco — 0 com dados, 1 sem dados.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndMov { get; set; }
}
