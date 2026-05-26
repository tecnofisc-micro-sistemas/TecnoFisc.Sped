using TecnoFisc.Sped.Core.Abstracoes;

namespace TecnoFisc.Sped.Core.Catalogo;

/// <summary>
/// Descreve um campo de um registro SPED — posição, tipo, restrições e como aplicá-lo
/// a uma instância. <see cref="Definidor"/> é o ponto que lê a propriedade durante o
/// parsing; <see cref="Serializar"/> é o ponto que a lê de volta para texto SPED durante
/// a geração.
/// </summary>
/// <remarks>
/// A API expõe apenas dois delegates compostos: o definidor recebe o conteúdo bruto do
/// campo como <see cref="ReadOnlySpan{Char}"/> (sem alocar string no chamador) e aplica
/// parse + atribuição em uma única operação; o serializador lê a propriedade já tipada e
/// devolve a representação canônica SPED. O caminho gerado pelo source generator (Stage 6)
/// implementa esses delegates inline com casts diretos para o tipo concreto, sem boxing.
/// O caminho reflexivo (<see cref="CatalogoBuilder"/>) compõe os mesmos delegates a partir
/// de delegados intermediários compilados via <c>Expression</c>; preserva a API externa mas
/// continua pagando boxing internamente — esperado, porque é o fallback.
/// </remarks>
public sealed class MetadadosCampo
{
    private readonly Action<RegistroSped, ReadOnlySpan<char>> _definidor;
    private readonly Func<RegistroSped, string> _serializador;

    public MetadadosCampo(
        string nome,
        int ordem,
        Type tipo,
        int tamanho,
        int decimais,
        bool obrigatorio,
        string? formato,
        Action<RegistroSped, ReadOnlySpan<char>> definidor,
        Func<RegistroSped, string> serializador,
        int desdeVersao = 0,
        bool capturaTudo = false,
        bool campoArquivo = false)
    {
        ArgumentNullException.ThrowIfNull(nome);
        ArgumentNullException.ThrowIfNull(tipo);
        ArgumentNullException.ThrowIfNull(definidor);
        ArgumentNullException.ThrowIfNull(serializador);

        Nome = nome;
        Ordem = ordem;
        Tipo = tipo;
        Tamanho = tamanho;
        Decimais = decimais;
        Obrigatorio = obrigatorio;
        Formato = formato;
        DesdeVersao = desdeVersao;
        CapturaTudo = capturaTudo;
        CampoArquivo = campoArquivo;
        _definidor = definidor;
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
    /// Primeira versão de leiaute em que o campo passa a existir. <c>0</c> = sempre presente.
    /// Origem em <see cref="Atributos.CampoSpedAttribute.DesdeVersao"/>.
    /// </summary>
    public int DesdeVersao { get; }

    /// <summary>
    /// Quando <c>true</c>, o parser captura tudo que restar na linha a partir deste campo,
    /// incluindo separadores <c>|</c> intermediários, como uma única string pipe-joined.
    /// Origem em <see cref="Atributos.CampoSpedAttribute.CapturaTudo"/>.
    /// </summary>
    public bool CapturaTudo { get; }

    /// <summary>
    /// Quando <c>true</c>, este é o campo-arquivo de um registro multi-linha (ver
    /// <see cref="MetadadosRegistro.TokenFimArquivo"/>): o parser captura tudo entre o separador
    /// anterior e o último <c>|</c> do registro montado, preservando <c>|</c> e CRLFs embutidos.
    /// Origem em <see cref="Atributos.CampoSpedAttribute.CampoArquivo"/>.
    /// </summary>
    public bool CampoArquivo { get; }

    /// <summary>
    /// Aplica o valor textual ao registro. Recebe o conteúdo do campo entre pipes (sem
    /// delimitadores). Vazio é interpretado conforme a nullabilidade do tipo de destino.
    /// </summary>
    public void Definidor(RegistroSped registro, ReadOnlySpan<char> valor)
        => _definidor(registro, valor);

    /// <summary>
    /// Lê o valor da propriedade no registro e devolve a representação canônica SPED.
    /// Valor nulo ou <c>default</c> em propriedade anulável vira string vazia.
    /// </summary>
    public string Serializar(RegistroSped registro)
    {
        ArgumentNullException.ThrowIfNull(registro);
        return _serializador(registro);
    }
}
