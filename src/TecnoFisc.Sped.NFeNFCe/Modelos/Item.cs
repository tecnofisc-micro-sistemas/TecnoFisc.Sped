namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Item da NF-e/NFC-e — grupo <c>det</c>. Combina o produto (<c>prod</c>) e seus impostos
/// (<c>imposto</c>). O número do item (<c>nItem</c>) é único dentro de uma mesma nota; a
/// identidade completa de um item no domínio é a chave de acesso da nota somada ao
/// <see cref="NItem"/> (a chave vive na <see cref="NFe"/> que possui este item).
/// </summary>
public sealed record Item
{
    /// <summary><c>nItem</c> — número sequencial do item na nota (1..990).</summary>
    public required int NItem { get; init; }

    /// <summary><c>prod</c> — dados do produto/serviço.</summary>
    public required Produto Prod { get; init; }

    /// <summary><c>imposto</c> — tributos incidentes sobre o item.</summary>
    public required Imposto Imposto { get; init; }
}
