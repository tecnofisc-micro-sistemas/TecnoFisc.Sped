// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSGCBSGDif> ReadTributacaoIBSCBSGIBSCBSGCBSGDifAsync(CancellationToken ct)
    {
        decimal? extraPDif = null;
        decimal? extraVDif = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "pDif":
                    extraPDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vDif":
                    extraVDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSGCBSGDif
        {
            PDif = extraPDif,
            VDif = extraVDif
        };
    }
}
