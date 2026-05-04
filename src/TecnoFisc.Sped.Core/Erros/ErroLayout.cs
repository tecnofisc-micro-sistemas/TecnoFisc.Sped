namespace TecnoFisc.Sped.Core.Erros;

/// <summary>
/// Falha de layout detectada na hierarquia ou na sequência dos registros (ex.: bloco sem
/// abertura, registro de nível inconsistente, código desconhecido pelo catálogo).
/// </summary>
public sealed record ErroLayout(
    long Linha,
    string? CodigoRegistro,
    string Mensagem)
{
    public override string ToString()
        => CodigoRegistro is null
            ? $"Linha {Linha}: {Mensagem}"
            : $"Linha {Linha} ({CodigoRegistro}): {Mensagem}";
}

public sealed class ErroLayoutSpedException : Exception
{
    public ErroLayout Erro { get; }

    public ErroLayoutSpedException(ErroLayout erro)
        : base(erro.ToString())
    {
        Erro = erro;
    }
}
