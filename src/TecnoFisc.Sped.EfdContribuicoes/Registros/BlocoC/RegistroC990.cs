using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C990 — Encerramento do Bloco C. Nível hierárquico 1, ocorrência única por
/// arquivo. Conforme Guia Prático v1.35, p. 192. O campo QTD_LIN_C é populado pelo
/// totalizador de blocos (Stage 3).
/// </summary>
[RegistroSped(Codigo = "C990", Nivel = 1, Bloco = "C")]
public sealed partial class RegistroC990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C990";

    /// <summary>Quantidade total de linhas do Bloco C, incluindo abertura e este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLinC { get; set; }
}
