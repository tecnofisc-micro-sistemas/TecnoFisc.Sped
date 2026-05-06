using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoA;

/// <summary>
/// Registro A001 — Abertura do Bloco A (serviços sujeitos ao ISS). Nível hierárquico 1,
/// ocorrência única por arquivo. Conforme Guia Prático v1.35, p. 95.
/// </summary>
[RegistroSped(Codigo = "A001", Nivel = 1, Bloco = "A")]
public sealed partial class RegistroA001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "A001";

    /// <summary>Indicador de movimento do bloco — 0 com dados, 1 sem dados.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndMov { get; set; }
}
