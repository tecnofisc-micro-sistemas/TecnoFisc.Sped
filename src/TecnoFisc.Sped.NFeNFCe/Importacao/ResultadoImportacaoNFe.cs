using TecnoFisc.Sped.Xml.Engine;

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record OrigemDocumentoNFe(string Arquivo, string? EntradaZip = null);

/// <summary>Exatamente um documento ou erro; a origem permite diagnóstico sem reter o lote.</summary>
public sealed class ResultadoImportacaoNFe
{
    private ResultadoImportacaoNFe(OrigemDocumentoNFe origem, IDocumentoFiscalXml? documento, Exception? erro)
    { Origem = origem; Documento = documento; Erro = erro; }
    public OrigemDocumentoNFe Origem { get; }
    public IDocumentoFiscalXml? Documento { get; }
    public Exception? Erro { get; }
    public bool IsSuccess => Documento is not null;
    internal static ResultadoImportacaoNFe Success(OrigemDocumentoNFe origin, IDocumentoFiscalXml document) => new(origin, document, null);
    internal static ResultadoImportacaoNFe Failure(OrigemDocumentoNFe origin, Exception error) => new(origin, null, error);
}
