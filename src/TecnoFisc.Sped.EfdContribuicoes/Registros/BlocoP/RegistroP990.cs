using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoP;

/// <summary>
/// Registro P990 — Encerramento do Bloco P. Nível hierárquico 1, ocorrência única por
/// arquivo. Conforme Guia Prático v1.35, p. 374. O campo QTD_LIN_P é populado pelo
/// totalizador de blocos (Stage 3).
/// </summary>
[RegistroSped(Codigo = "P990", Nivel = 1, Bloco = "P")]
public sealed partial class RegistroP990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "P990";

    /// <summary>Quantidade total de linhas do Bloco P, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinP { get; set; }
}
