using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.Core.Catalogo;

/// <summary>
/// Registro extensível de conversores string → objeto fortemente tipado, usado pelo
/// <see cref="CatalogoBuilder"/> para tipos que não são primitivos da BCL. Pré-popula
/// os value objects fiscais expostos pelo Core e aceita extensões via
/// <see cref="Registrar{T}"/>.
/// </summary>
public static class ConversoresPrimitivosCatalogo
{
    private static readonly ConcurrentDictionary<Type, Func<string, object?>> _conversores = new();

    static ConversoresPrimitivosCatalogo()
    {
        Registrar(static s => Cnpj.Criar(s));
        Registrar(static s => Cpf.Criar(s));
        Registrar(static s => Cfop.Criar(s));
        Registrar(static s => Ncm.Criar(s));
        Registrar(static s => ChaveAcesso.Criar(s));
        Registrar(static s => InscricaoEstadual.Criar(s));
        // Cst depende de TipoTributo no momento da construção; o consumidor que precisar
        // dele em um campo SPED automatizado deve registrar um conversor com o tributo
        // adequado para o seu contexto.
    }

    /// <summary>
    /// Registra (ou substitui) um conversor para o tipo <typeparamref name="T"/>. A entrada
    /// recebe o valor textual já extraído entre dois pipes (sem o delimitador).
    /// </summary>
    public static void Registrar<T>(Func<string, T> conversor) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(conversor);
        _conversores[typeof(T)] = s => conversor(s);
    }

    public static bool TentarObter(Type tipo, [MaybeNullWhen(false)] out Func<string, object?> conversor)
        => _conversores.TryGetValue(tipo, out conversor);

    /// <summary>Remove um conversor previamente registrado. Útil em cenários de teste.</summary>
    public static bool Remover(Type tipo) => _conversores.TryRemove(tipo, out _);
}
