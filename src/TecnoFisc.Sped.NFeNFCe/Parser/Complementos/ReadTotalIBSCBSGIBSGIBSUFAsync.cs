// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TotalIBSCBSGIBSGIBSUF> ReadTotalIBSCBSGIBSGIBSUFAsync(CancellationToken ct)
    {
        decimal? extraVDif = null;
        decimal? extraVDevTrib = null;
        decimal? extraVIBSUF = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vDif":
                    extraVDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vDevTrib":
                    extraVDevTrib = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vIBSUF":
                    extraVIBSUF = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TotalIBSCBSGIBSGIBSUF
        {
            VDif = extraVDif,
            VDevTrib = extraVDevTrib,
            VIBSUF = extraVIBSUF
        };
    }
}
