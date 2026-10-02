// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoReten> ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoRetenAsync(CancellationToken ct)
    {
        decimal? extraVBCMonoReten = null;
        decimal? extraPAliqMonoReten = null;
        decimal? extraVIBSMonoReten = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vBCMonoReten":
                    extraVBCMonoReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "pAliqMonoReten":
                    extraPAliqMonoReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vIBSMonoReten":
                    extraVIBSMonoReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoReten
        {
            VBCMonoReten = extraVBCMonoReten,
            PAliqMonoReten = extraPAliqMonoReten,
            VIBSMonoReten = extraVIBSMonoReten
        };
    }
}
