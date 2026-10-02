// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoReten> ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoRetenAsync(CancellationToken ct)
    {
        decimal? extraQBCMonoReten = null;
        decimal? extraAdRemIBSReten = null;
        decimal? extraVIBSMonoReten = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "qBCMonoReten":
                    extraQBCMonoReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "adRemIBSReten":
                    extraAdRemIBSReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vIBSMonoReten":
                    extraVIBSMonoReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoReten
        {
            QBCMonoReten = extraQBCMonoReten,
            AdRemIBSReten = extraAdRemIBSReten,
            VIBSMonoReten = extraVIBSMonoReten
        };
    }
}
