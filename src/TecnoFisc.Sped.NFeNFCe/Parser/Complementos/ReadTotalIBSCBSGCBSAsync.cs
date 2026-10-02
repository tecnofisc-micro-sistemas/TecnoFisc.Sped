// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TotalIBSCBSGCBS> ReadTotalIBSCBSGCBSAsync(CancellationToken ct)
    {
        decimal? extraVDif = null;
        decimal? extraVDevTrib = null;
        decimal? extraVCBS = null;
        decimal? extraVCredPres = null;
        decimal? extraVCredPresCondSus = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vDif": extraVDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vDevTrib": extraVDevTrib = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vCBS": extraVCBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vCredPres": extraVCredPres = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vCredPresCondSus": extraVCredPresCondSus = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TotalIBSCBSGCBS
        {
            VDif = extraVDif,
            VDevTrib = extraVDevTrib,
            VCBS = extraVCBS,
            VCredPres = extraVCredPres,
            VCredPresCondSus = extraVCredPresCondSus
        };
    }
}
