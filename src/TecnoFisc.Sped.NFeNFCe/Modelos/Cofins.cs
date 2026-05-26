using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Base abstrata das variantes de tributação COFINS de um item (grupo <c>COFINS</c>).
/// O consumidor faz pattern matching sobre o tipo concreto:
/// <see cref="CofinsAliq"/>, <see cref="CofinsQtde"/>, <see cref="CofinsNt"/> ou <see cref="CofinsOutr"/>.
/// </summary>
public abstract record Cofins
{
    /// <summary>
    /// <c>CST</c> — código de situação tributária do COFINS (2 dígitos, conforme
    /// <see cref="TipoTributo.Cofins"/>). Sem prefixo <c>orig</c> — COFINS não usa origem.
    /// </summary>
    public required Cst CST { get; init; }
}
