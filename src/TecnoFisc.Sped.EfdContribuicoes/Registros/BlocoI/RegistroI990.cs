using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoI;

/// <summary>
/// Registro I990 — Encerramento do Bloco I. Nível hierárquico 1, ocorrência única por
/// arquivo. Conforme Guia Prático v1.35, p. 294. O campo QTD_LIN_I é populado pelo
/// totalizador de blocos (Stage 3).
/// </summary>
[RegistroSped(Codigo = "I990", Nivel = 1, Bloco = "I")]
public sealed partial class RegistroI990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I990";

    /// <summary>Quantidade total de linhas do Bloco I, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinI { get; set; }
}
