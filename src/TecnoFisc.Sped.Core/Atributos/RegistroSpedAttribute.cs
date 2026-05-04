namespace TecnoFisc.Sped.Core.Atributos;

/// <summary>
/// Marca uma classe como representante de um registro SPED. Os campos do registro são marcados
/// individualmente com <see cref="CampoSpedAttribute"/>. As propriedades do atributo descrevem
/// o registro no catálogo: código (ex.: "C100"), nível hierárquico (raiz = 0) e bloco.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RegistroSpedAttribute : Attribute
{
    /// <summary>Código do registro como aparece no arquivo SPED (ex.: "0000", "C100", "9999").</summary>
    public required string Codigo { get; init; }

    /// <summary>Nível hierárquico: 0 = raiz, 1 = abertura/fechamento de bloco, 2+ = detalhe.</summary>
    public required int Nivel { get; init; }

    /// <summary>Identificador do bloco ao qual o registro pertence (ex.: "0", "C", "9").</summary>
    public required string Bloco { get; init; }
}
