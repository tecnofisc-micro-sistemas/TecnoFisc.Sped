// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TotalIBSCBS> ReadTotalIBSCBSAsync(CancellationToken ct)
    {
        decimal? extraVBCIBSCBS = null;
        TotalIBSCBSGIBS? extraGIBS = null;
        TotalIBSCBSGCBS? extraGCBS = null;
        TotalIBSCBSGMono? extraGMono = null;
        TotalIBSCBSGEstornoCred? extraGEstornoCred = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vBCIBSCBS": extraVBCIBSCBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "gIBS": extraGIBS = await ReadTotalIBSCBSGIBSAsync(ct).ConfigureAwait(false); return true;
                case "gCBS": extraGCBS = await ReadTotalIBSCBSGCBSAsync(ct).ConfigureAwait(false); return true;
                case "gMono": extraGMono = await ReadTotalIBSCBSGMonoAsync(ct).ConfigureAwait(false); return true;
                case "gEstornoCred": extraGEstornoCred = await ReadTotalIBSCBSGEstornoCredAsync(ct).ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TotalIBSCBS
        {
            VBCIBSCBS = extraVBCIBSCBS,
            GIBS = extraGIBS,
            GCBS = extraGCBS,
            GMono = extraGMono,
            GEstornoCred = extraGEstornoCred
        };
    }
}
