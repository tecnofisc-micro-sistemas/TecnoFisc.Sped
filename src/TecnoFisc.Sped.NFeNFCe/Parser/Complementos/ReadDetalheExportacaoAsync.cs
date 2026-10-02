// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<DetalheExportacao> ReadDetalheExportacaoAsync(CancellationToken ct)
    {
        string? extraNDraw = null;
        DetalheExportacaoExportInd? extraExportInd = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "nDraw":
                    extraNDraw = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "exportInd":
                    extraExportInd = await ReadDetalheExportacaoExportIndAsync(ct).ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new DetalheExportacao
        {
            NDraw = extraNDraw,
            ExportInd = extraExportInd
        };
    }
}
