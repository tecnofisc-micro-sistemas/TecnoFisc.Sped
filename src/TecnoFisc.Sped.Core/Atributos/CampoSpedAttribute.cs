namespace TecnoFisc.Sped.Core.Atributos;

/// <summary>
/// Marca uma propriedade como um campo persistido em um registro SPED. A ordem espelha a
/// numeração "Nº" das tabelas do Guia Prático: REG é o campo Nº 1 (não recebe atributo,
/// é resolvido pelo próprio código do registro) e os demais começam em <c>Ordem = 2</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CampoSpedAttribute : Attribute
{
    /// <summary>
    /// Posição do campo no layout, idêntica à coluna "Nº" do Guia Prático. REG ocupa a
    /// posição 1 (implícita); o primeiro campo declarado em código começa em 2.
    /// </summary>
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
