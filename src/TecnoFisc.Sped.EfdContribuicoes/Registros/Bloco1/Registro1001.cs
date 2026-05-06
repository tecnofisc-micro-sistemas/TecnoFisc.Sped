using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1001 — Abertura do Bloco 1 (complemento da escrituração). Nível hierárquico 1,
/// ocorrência única por arquivo. Conforme Guia Prático v1.35, p. 375.
/// </summary>
[RegistroSped(Codigo = "1001", Nivel = 1, Bloco = "1")]
public sealed partial class Registro1001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1001";

    /// <summary>Indicador de movimento do bloco — 0 com dados, 1 sem dados.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndMov { get; set; }
}
