using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoF;

/// <summary>
/// Registro F990 — Encerramento do Bloco F. Nível hierárquico 1, ocorrência única por
/// arquivo. Conforme Guia Prático v1.35, p. 283. O campo QTD_LIN_F é populado pelo
/// totalizador de blocos (Stage 3).
/// </summary>
[RegistroSped(Codigo = "F990", Nivel = 1, Bloco = "F")]
public sealed partial class RegistroF990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "F990";

    /// <summary>Quantidade total de linhas do Bloco F, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinF { get; set; }
}
