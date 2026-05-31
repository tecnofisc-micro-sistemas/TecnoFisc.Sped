using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D990 — Encerramento do Bloco D. Nível hierárquico 1, ocorrência única por
/// arquivo. Conforme Guia Prático v1.35, p. 228. O campo QTD_LIN_D é populado pelo
/// totalizador de blocos (Stage 3).
/// </summary>
[RegistroSped(Codigo = "D990", Nivel = 1, Bloco = "D")]
public sealed partial class RegistroD990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D990";

    /// <summary>Quantidade total de linhas do Bloco D, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinD { get; set; }
}
