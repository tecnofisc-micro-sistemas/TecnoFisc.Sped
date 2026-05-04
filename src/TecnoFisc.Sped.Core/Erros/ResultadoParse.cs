using System.Diagnostics.CodeAnalysis;

namespace TecnoFisc.Sped.Core.Erros;

#pragma warning disable CA1000 // métodos estáticos em tipo genérico — padrão idiomático para Result<T>

/// <summary>
/// Resultado de uma operação de parsing que pode falhar de forma esperada (linha malformada,
/// campo fora do tipo, etc.). Para erros de programação use exceções diretamente.
/// </summary>
public readonly struct ResultadoParse<T>
{
    private readonly T? _valor;
    private readonly IReadOnlyList<ErroFormato>? _erros;

    private ResultadoParse(T? valor, IReadOnlyList<ErroFormato>? erros, bool sucesso)
    {
        _valor = valor;
        _erros = erros;
        Sucesso = sucesso;
    }

    /// <summary>Indica se o parsing produziu um valor válido.</summary>
    public bool Sucesso { get; }

    /// <summary>Falha = não-sucesso. Atalho para legibilidade.</summary>
    public bool Falha => !Sucesso;

    /// <summary>Valor produzido em caso de sucesso. Lançará se acessado em falha.</summary>
    public T Valor => Sucesso
        ? _valor!
        : throw new InvalidOperationException("ResultadoParse em estado de falha não possui valor.");

    /// <summary>Lista de erros acumulados em caso de falha.</summary>
    public IReadOnlyList<ErroFormato> Erros => _erros ?? Array.Empty<ErroFormato>();

    public static ResultadoParse<T> Ok(T valor) => new(valor, null, sucesso: true);

    public static ResultadoParse<T> Falhar(ErroFormato erro) => new(default, [erro], sucesso: false);

    public static ResultadoParse<T> Falhar(IReadOnlyList<ErroFormato> erros)
    {
        ArgumentNullException.ThrowIfNull(erros);
        return new(default, erros, sucesso: false);
    }

    public bool TentarObter([MaybeNullWhen(false)] out T valor)
    {
        valor = _valor;
        return Sucesso;
    }
}
