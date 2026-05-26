using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Base abstrata das variantes de tributação IPI de um item (grupo <c>IPI/IPITrib</c> ou
/// <c>IPI/IPINT</c>). O consumidor faz pattern matching sobre o tipo concreto.
/// </summary>
public abstract record IpiTributacao
{
    /// <summary>
    /// <c>CST</c> — código de situação tributária do IPI (2 dígitos, conforme
    /// <see cref="TipoTributo.Ipi"/>). Sem prefixo <c>orig</c> — IPI não usa origem.
    /// </summary>
    public required Cst CST { get; init; }
}
