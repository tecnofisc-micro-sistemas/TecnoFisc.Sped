// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoReten> ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoRetenAsync(CancellationToken ct)
    {
        decimal? extraQBCMonoReten = null;
        decimal? extraAdRemCBSReten = null;
        decimal? extraVCBSMonoReten = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "qBCMonoReten": extraQBCMonoReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "adRemCBSReten": extraAdRemCBSReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vCBSMonoReten": extraVCBSMonoReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoReten
        {
            QBCMonoReten = extraQBCMonoReten,
            AdRemCBSReten = extraAdRemCBSReten,
            VCBSMonoReten = extraVCBSMonoReten
        };
    }
}
