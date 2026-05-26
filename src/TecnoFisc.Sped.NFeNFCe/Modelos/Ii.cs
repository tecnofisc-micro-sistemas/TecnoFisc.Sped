namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Grupo <c>II</c> — dados do Imposto de Importação de um item.
/// Elemento irmão de <c>ICMS</c>/<c>IPI</c> dentro de <c>imposto</c>.
/// Não possui CST nem hierarquia polimórfica: é um registro plano.
/// </summary>
public sealed record Ii
{
    /// <summary><c>vBC</c> — base de cálculo do Imposto de Importação.</summary>
    public required decimal VBC { get; init; }

    /// <summary><c>vDespAdu</c> — valor das despesas aduaneiras.</summary>
    public required decimal VDespAdu { get; init; }

    /// <summary><c>vII</c> — valor do Imposto de Importação.</summary>
    public required decimal VII { get; init; }

    /// <summary><c>vIOF</c> — valor do Imposto sobre Operações Financeiras.</summary>
    public required decimal VIOF { get; init; }
}
