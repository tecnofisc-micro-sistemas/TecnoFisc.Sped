namespace TecnoFisc.Sped.Core.Atributos;

/// <summary>
/// Marca uma propriedade como um campo persistido em um registro SPED. A ordem reflete a
/// posição da coluna dentro da linha pipe-delimitada, contada a partir do primeiro campo
/// posterior ao código do registro (REG é considerado coluna 0 e não é marcado).
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CampoSpedAttribute : Attribute
{
    /// <summary>Posição do campo dentro da linha (1-based, REG fica na posição 0).</summary>
    public required int Ordem { get; init; }

    /// <summary>Tamanho máximo declarado pelo layout. <c>0</c> indica tamanho livre.</summary>
    public int Tamanho { get; init; }

    /// <summary>Quantidade de casas decimais para campos numéricos. <c>0</c> para inteiros e textos.</summary>
    public int Decimais { get; init; }

    /// <summary>Indica se o layout exige preenchimento; falso permite valor vazio.</summary>
    public bool Obrigatorio { get; init; }

    /// <summary>
    /// Formato textual auxiliar para datas e similares. Para datas use "ddMMyyyy" (padrão SPED)
    /// ou "MMyyyy" para campos de período. Outros tipos ignoram.
    /// </summary>
    public string? Formato { get; init; }
}
