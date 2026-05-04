using System.Diagnostics.CodeAnalysis;

using TecnoFisc.Sped.Core.Abstracoes;

namespace TecnoFisc.Sped.Core.Catalogo;

/// <summary>
/// Base para catálogos concretos. Aceita um dicionário pré-populado com
/// <see cref="StringComparer.Ordinal"/> e usa <see cref="Dictionary{TKey, TValue}.AlternateLookup{TAlternateKey}"/>
/// (.NET 9+) para consultar por <see cref="ReadOnlySpan{Char}"/> sem alocar string.
/// Catálogos gerados em tempo de compilação (Stage 6) herdam desta classe.
/// </summary>
public abstract class CatalogoSpedBase : IRegistroSpedCatalogo
{
    private readonly Dictionary<string, MetadadosRegistro> _registros;
    private readonly Dictionary<string, MetadadosRegistro>.AlternateLookup<ReadOnlySpan<char>> _consultaPorSpan;

    protected CatalogoSpedBase(Dictionary<string, MetadadosRegistro> registros)
    {
        ArgumentNullException.ThrowIfNull(registros);
        _registros = registros;
        _consultaPorSpan = registros.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public bool TentarObter(ReadOnlySpan<char> codigo, [MaybeNullWhen(false)] out MetadadosRegistro metadados)
        => _consultaPorSpan.TryGetValue(codigo, out metadados);

    public IEnumerable<MetadadosRegistro> EnumerarRegistros() => _registros.Values;
}
