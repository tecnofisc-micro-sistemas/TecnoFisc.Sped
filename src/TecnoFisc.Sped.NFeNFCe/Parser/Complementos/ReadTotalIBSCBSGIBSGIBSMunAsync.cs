// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TotalIBSCBSGIBSGIBSMun> ReadTotalIBSCBSGIBSGIBSMunAsync(CancellationToken ct)
    {
        decimal? extraVDif = null;
        decimal? extraVDevTrib = null;
        decimal? extraVIBSMun = null;
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
                case "vIBSMun":
                    extraVIBSMun = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TotalIBSCBSGIBSGIBSMun
        {
            VDif = extraVDif,
            VDevTrib = extraVDevTrib,
            VIBSMun = extraVIBSMun
        };
    }
}
