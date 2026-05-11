using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco9;

/// <summary>
/// Registro 9001 — Abertura do Bloco 9 (controle e encerramento do arquivo digital).
/// Nível hierárquico 1, ocorrência única por arquivo. Conforme Guia Prático v1.35, p. 415.
/// </summary>
[RegistroSped(Codigo = "9001", Nivel = 1, Bloco = "9")]
public sealed partial class Registro9001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9001";

    /// <summary>Indicador de movimento do bloco — 0 com dados, 1 sem dados.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndMov { get; set; }
}
