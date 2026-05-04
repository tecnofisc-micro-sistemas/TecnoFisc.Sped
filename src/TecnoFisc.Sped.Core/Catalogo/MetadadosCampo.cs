using TecnoFisc.Sped.Core.Abstracoes;

namespace TecnoFisc.Sped.Core.Catalogo;

/// <summary>
/// Descreve um campo de um registro SPED — posição, tipo, restrições e como aplicá-lo
/// a uma instância. <see cref="Definidor"/> é o único ponto que toca a propriedade do
/// registro durante o parsing (no Stage 2 via reflexão compilada em delegate; no Stage 6
/// substituído por código gerado).
/// </summary>
public sealed class MetadadosCampo
{
    private readonly Func<string, object?> _conversor;
    private readonly Action<RegistroSped, object?> _setter;

    internal MetadadosCampo(
        string nome,
        int ordem,
        Type tipo,
        int tamanho,
        int decimais,
        bool obrigatorio,
        string? formato,
        Func<string, object?> conversor,
        Action<RegistroSped, object?> setter)
    {
        Nome = nome;
        Ordem = ordem;
        Tipo = tipo;
        Tamanho = tamanho;
        Decimais = decimais;
        Obrigatorio = obrigatorio;
        Formato = formato;
        _conversor = conversor;
        _setter = setter;
    }

    public string Nome { get; }
    public int Ordem { get; }
    public Type Tipo { get; }
    public int Tamanho { get; }
    public int Decimais { get; }
    public bool Obrigatorio { get; }
    public string? Formato { get; }

    /// <summary>
    /// Aplica o valor textual ao registro. Espera o conteúdo do campo (entre pipes), sem
    /// delimitadores. Vazio é interpretado conforme a nullabilidade da propriedade.
    /// </summary>
    public void Definidor(RegistroSped registro, ReadOnlySpan<char> valor)
    {
        // Stage 2 fallback: aloca string. Stage 6 (source generator) elimina essa alocação.
        string? texto = valor.IsEmpty ? null : valor.ToString();
        object? convertido = _conversor(texto ?? string.Empty);
        _setter(registro, convertido);
    }
}
