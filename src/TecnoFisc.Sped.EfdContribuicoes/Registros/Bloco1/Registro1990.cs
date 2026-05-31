using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1990 — Encerramento do Bloco 1. Nível hierárquico 1, ocorrência única por
/// arquivo. Conforme Guia Prático v1.35, p. 413. O campo QTD_LIN_1 é populado pelo
/// totalizador de blocos (Stage 3).
/// </summary>
[RegistroSped(Codigo = "1990", Nivel = 1, Bloco = "1")]
public sealed partial class Registro1990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1990";

    /// <summary>Quantidade total de linhas do Bloco 1, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLin1 { get; set; }
}
