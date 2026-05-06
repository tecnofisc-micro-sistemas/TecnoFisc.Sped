using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco9;

/// <summary>
/// Registro 9999 — Encerramento do Arquivo Digital. Nível hierárquico 0 (raiz, junto com
/// 0000), ocorrência única por arquivo. Conforme Guia Prático v1.35, p. 416. O campo
/// QTD_LIN inclui o próprio 9999 e é populado pelo totalizador (Stage 3).
/// </summary>
[RegistroSped(Codigo = "9999", Nivel = 0, Bloco = "9")]
public sealed partial class Registro9999 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9999";

    /// <summary>Quantidade total de linhas do arquivo, incluindo este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLin { get; set; }
}
