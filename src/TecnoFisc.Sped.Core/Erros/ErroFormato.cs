namespace TecnoFisc.Sped.Core.Erros;

/// <summary>
/// Falha de formato detectada na linha durante o parsing. Inclui número da linha (1-based),
/// código do registro (quando identificável), nome do campo (quando aplicável) e mensagem.
/// </summary>
public sealed record ErroFormato(
    long Linha,
    string? CodigoRegistro,
    string? Campo,
    string Mensagem)
{
    public override string ToString()
        => CodigoRegistro is null
            ? $"Linha {Linha}: {Mensagem}"
            : Campo is null
                ? $"Linha {Linha} ({CodigoRegistro}): {Mensagem}"
                : $"Linha {Linha} ({CodigoRegistro}.{Campo}): {Mensagem}";
}

/// <summary>
/// Exceção lançada por leitores que param no primeiro erro de formato.
/// Use o overload com <see cref="ResultadoParse{T}"/> para coletar todos os erros de uma só vez.
/// </summary>
public sealed class ErroFormatoSpedException : Exception
{
    public ErroFormato Erro { get; }

    public ErroFormatoSpedException(ErroFormato erro)
        : base(erro.ToString())
    {
        Erro = erro;
    }

    public ErroFormatoSpedException(ErroFormato erro, Exception interna)
        : base(erro.ToString(), interna)
    {
        Erro = erro;
    }
}
