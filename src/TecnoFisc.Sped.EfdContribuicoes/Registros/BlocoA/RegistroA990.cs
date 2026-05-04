using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoA;

/// <summary>
/// Registro A990 — Encerramento do Bloco A. Nível hierárquico 1, ocorrência única por
/// arquivo. Conforme Guia Prático v1.35, p. 104. O campo QTD_LIN_A é populado pelo
/// totalizador de blocos (Stage 3).
/// </summary>
[RegistroSped(Codigo = "A990", Nivel = 1, Bloco = "A")]
public sealed partial class RegistroA990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "A990";

    /// <summary>Quantidade total de linhas do Bloco A, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinA { get; set; }
}
