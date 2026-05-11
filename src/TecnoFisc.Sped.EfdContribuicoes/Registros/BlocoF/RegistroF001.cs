using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F001 — Abertura do Bloco F (demais documentos e operações).
/// Nível hierárquico 1, ocorrência única por arquivo. Conforme Guia Prático v1.35, p. 229.
/// </summary>
[RegistroSped(Codigo = "F001", Nivel = 1, Bloco = "F")]
public sealed partial class RegistroF001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "F001";

    /// <summary>Indicador de movimento do bloco — 0 com dados, 1 sem dados.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndMov { get; set; }
}
