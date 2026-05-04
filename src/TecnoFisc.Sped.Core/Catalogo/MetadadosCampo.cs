using TecnoFisc.Sped.Core.Abstracoes;

namespace TecnoFisc.Sped.Core.Catalogo;

/// <summary>
/// Descreve um campo de um registro SPED — posição, tipo, restrições e como aplicá-lo
/// a uma instância. <see cref="Definidor"/> é o ponto que lê a propriedade durante o
/// parsing; <see cref="Serializar"/> é o ponto que a lê de volta para texto SPED durante
/// a geração. No Stage 2/3 ambos são compilados em delegate via reflexão; no Stage 6
/// o source generator substitui o catálogo reflexivo por código gerado.
/// </summary>
public sealed class MetadadosCampo
{
    private readonly Func<string, object?> _conversor;
    private readonly Action<RegistroSped, object?> _setter;
    private readonly Func<RegistroSped, object?> _getter;
    private readonly Func<object, string> _serializador;

    internal MetadadosCampo(
        string nome,
        int ordem,
        Type tipo,
        int tamanho,
        int decimais,
        bool obrigatorio,
        string? formato,
        Func<string, object?> conversor,
        Action<RegistroSped, object?> setter,
        Func<RegistroSped, object?> getter,
        Func<object, string> serializador)
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
        _getter = getter;
        _serializador = serializador;
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

    /// <summary>
    /// Lê o valor da propriedade no registro e devolve a representação canônica SPED. Valor
    /// nulo ou <c>default</c> em propriedade anulável vira string vazia.
    /// </summary>
    public string Serializar(RegistroSped registro)
    {
        ArgumentNullException.ThrowIfNull(registro);
        object? valor = _getter(registro);
        return valor is null ? string.Empty : _serializador(valor);
    }
}
