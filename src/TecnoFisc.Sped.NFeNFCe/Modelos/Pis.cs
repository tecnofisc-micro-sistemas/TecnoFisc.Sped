using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Base abstrata das variantes de tributação PIS de um item (grupo <c>PIS</c>).
/// O consumidor faz pattern matching sobre o tipo concreto:
/// <see cref="PisAliq"/>, <see cref="PisQtde"/>, <see cref="PisNt"/> ou <see cref="PisOutr"/>.
/// </summary>
public abstract record Pis
{
    /// <summary>
    /// <c>CST</c> — código de situação tributária do PIS (2 dígitos, conforme
    /// <see cref="TipoTributo.Pis"/>). Sem prefixo <c>orig</c> — PIS não usa origem.
    /// </summary>
    public required Cst CST { get; init; }
}
