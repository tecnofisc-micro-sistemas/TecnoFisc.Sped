using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco9;

/// <summary>
/// Registro 9990 — Encerramento do Bloco 9. Nível hierárquico 1, ocorrência única por
/// arquivo. Conforme Guia Prático v1.35, p. 415. O campo QTD_LIN_9 inclui o próprio
/// registro 9999, conforme a regra do guia. Populado pelo totalizador de blocos (Stage 3).
/// </summary>
[RegistroSped(Codigo = "9990", Nivel = 1, Bloco = "9")]
public sealed partial class Registro9990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9990";

    /// <summary>Quantidade total de linhas do Bloco 9, incluindo abertura, este registro e o 9999.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLin9 { get; set; }
}
