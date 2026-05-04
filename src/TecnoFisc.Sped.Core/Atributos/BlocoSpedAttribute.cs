namespace TecnoFisc.Sped.Core.Atributos;

/// <summary>
/// Marca uma classe que agrupa registros pertencentes a um bloco SPED. Usado pelos modelos
/// de arquivo de cada projeto (ex.: Bloco0, BlocoC) para expor coleções tipadas.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class BlocoSpedAttribute : Attribute
{
    /// <summary>Identificador do bloco (ex.: "0", "C", "F", "9").</summary>
    public required string Identificador { get; init; }
}
