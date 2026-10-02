// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<DetalheExportacaoExportInd> ReadDetalheExportacaoExportIndAsync(CancellationToken ct)
    {
        string? extraNRE = null;
        string? extraChNFe = null;
        decimal? extraQExport = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "nRE":
                    extraNRE = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "chNFe":
                    extraChNFe = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "qExport":
                    extraQExport = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new DetalheExportacaoExportInd
        {
            NRE = extraNRE,
            ChNFe = extraChNFe,
            QExport = extraQExport
        };
    }
}
