namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>Um orçamento configurado de leitura foi excedido.</summary>
public sealed class LimiteLeituraException : IOException
{
    public LimiteLeituraException() { }
    public LimiteLeituraException(string message) : base(message) { }
    public LimiteLeituraException(string message, Exception innerException) : base(message, innerException) { }
}
