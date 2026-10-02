// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TotalIBSCBSGMono> ReadTotalIBSCBSGMonoAsync(CancellationToken ct)
    {
        decimal? extraVIBSMono = null;
        decimal? extraVCBSMono = null;
        decimal? extraVIBSMonoReten = null;
        decimal? extraVCBSMonoReten = null;
        decimal? extraVIBSMonoRet = null;
        decimal? extraVCBSMonoRet = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vIBSMono":
                    extraVIBSMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vCBSMono":
                    extraVCBSMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vIBSMonoReten":
                    extraVIBSMonoReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vCBSMonoReten":
                    extraVCBSMonoReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vIBSMonoRet":
                    extraVIBSMonoRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vCBSMonoRet":
                    extraVCBSMonoRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TotalIBSCBSGMono
        {
            VIBSMono = extraVIBSMono,
            VCBSMono = extraVCBSMono,
            VIBSMonoReten = extraVIBSMonoReten,
            VCBSMonoReten = extraVCBSMonoReten,
            VIBSMonoRet = extraVIBSMonoRet,
            VCBSMonoRet = extraVCBSMonoRet
        };
    }
}
