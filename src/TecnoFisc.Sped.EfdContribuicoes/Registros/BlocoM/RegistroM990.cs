using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoM;

/// <summary>
/// Registro M990 — Encerramento do Bloco M. Nível hierárquico 1, ocorrência única por
/// arquivo. Conforme Guia Prático v1.35, p. 362. O campo QTD_LIN_M é populado pelo
/// totalizador de blocos (Stage 3).
/// </summary>
[RegistroSped(Codigo = "M990", Nivel = 1, Bloco = "M")]
public sealed partial class RegistroM990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M990";

    /// <summary>Quantidade total de linhas do Bloco M, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinM { get; set; }
}
