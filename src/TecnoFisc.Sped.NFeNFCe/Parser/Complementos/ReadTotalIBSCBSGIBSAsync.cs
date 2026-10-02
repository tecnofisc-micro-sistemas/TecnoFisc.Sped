// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TotalIBSCBSGIBS> ReadTotalIBSCBSGIBSAsync(CancellationToken ct)
    {
        TotalIBSCBSGIBSGIBSUF? extraGIBSUF = null;
        TotalIBSCBSGIBSGIBSMun? extraGIBSMun = null;
        decimal? extraVIBS = null;
        decimal? extraVCredPres = null;
        decimal? extraVCredPresCondSus = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "gIBSUF": extraGIBSUF = await ReadTotalIBSCBSGIBSGIBSUFAsync(ct).ConfigureAwait(false); return true;
                case "gIBSMun": extraGIBSMun = await ReadTotalIBSCBSGIBSGIBSMunAsync(ct).ConfigureAwait(false); return true;
                case "vIBS": extraVIBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vCredPres": extraVCredPres = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vCredPresCondSus": extraVCredPresCondSus = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TotalIBSCBSGIBS
        {
            GIBSUF = extraGIBSUF,
            GIBSMun = extraGIBSMun,
            VIBS = extraVIBS,
            VCredPres = extraVCredPres,
            VCredPresCondSus = extraVCredPresCondSus
        };
    }
}
