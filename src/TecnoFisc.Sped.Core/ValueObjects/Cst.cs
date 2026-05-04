namespace TecnoFisc.Sped.Core.ValueObjects;

/// <summary>
/// CST — Código de Situação Tributária. O comprimento depende do tributo:
/// ICMS = 3 caracteres, IPI/PIS/COFINS = 2 caracteres. Em SPED é gravado apenas com dígitos.
/// </summary>
public readonly struct Cst : IEquatable<Cst>
{
    private readonly string? _valor;

    public TipoTributo Tributo { get; }

    private Cst(string valor, TipoTributo tributo)
    {
        _valor = valor;
        Tributo = tributo;
    }

    /// <exception cref="FormatException">Quando o valor não é um CST válido para o tributo.</exception>
    public static Cst Criar(ReadOnlySpan<char> valor, TipoTributo tributo)
    {
        if (!TentarCriar(valor, tributo, out var cst))
            throw new FormatException($"Valor não é um CST válido para {tributo}: '{valor}'.");
        return cst;
    }

    public static bool TentarCriar(ReadOnlySpan<char> valor, TipoTributo tributo, out Cst cst)
    {
        int tamanho = TamanhoEsperado(tributo);
        Span<char> digitos = stackalloc char[16];
        var destino = digitos[..tamanho];

        if (!DigitosSomenteHelper.TentarExtrair(valor, destino))
        {
            cst = default;
            return false;
        }

        cst = new Cst(new string(destino), tributo);
        return true;
    }

    public static int TamanhoEsperado(TipoTributo tributo) => tributo switch
    {
        TipoTributo.Icms => 3,
        TipoTributo.Ipi => 2,
        TipoTributo.Pis => 2,
        TipoTributo.Cofins => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(tributo)),
    };

    public override string ToString() => _valor ?? string.Empty;

    public bool Equals(Cst other)
        => Tributo == other.Tributo && string.Equals(_valor, other._valor, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is Cst c && Equals(c);
    public override int GetHashCode() => HashCode.Combine(Tributo, _valor);
    public static bool operator ==(Cst left, Cst right) => left.Equals(right);
    public static bool operator !=(Cst left, Cst right) => !left.Equals(right);
}
