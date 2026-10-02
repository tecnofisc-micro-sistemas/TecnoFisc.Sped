namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>Falha no callback do consumidor. InnerException mantém a causa; nunca é tratada como XML inválido.</summary>
public sealed class FalhaCallbackXmlException : Exception
{
    public FalhaCallbackXmlException() { }
    public FalhaCallbackXmlException(string message) : base(message) { }
    public FalhaCallbackXmlException(string message, Exception innerException) : base(message, innerException) { }
}
