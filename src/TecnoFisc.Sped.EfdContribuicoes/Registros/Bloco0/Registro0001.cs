using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0001 — Abertura do Bloco 0. Nível hierárquico 1, ocorrência única por arquivo.
/// Conforme Guia Prático v1.35, p. 69. Indica se o Bloco 0 contém ou não registros
/// de movimento (campo IND_MOV).
/// </summary>
[RegistroSped(Codigo = "0001", Nivel = 1, Bloco = "0")]
public sealed partial class Registro0001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0001";

    /// <summary>Indicador de movimento do bloco — 0 com dados, 1 sem dados.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndMov { get; set; }
}
