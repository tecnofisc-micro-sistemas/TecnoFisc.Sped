// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBS> ReadTributacaoIBSCBSGIBSCBSAsync(CancellationToken ct)
    {
        decimal? extraVBC = null;
        TributacaoIBSCBSGIBSCBSGIBSUF? extraGIBSUF = null;
        TributacaoIBSCBSGIBSCBSGIBSMun? extraGIBSMun = null;
        decimal? extraVIBS = null;
        TributacaoIBSCBSGIBSCBSGCBS? extraGCBS = null;
        TributacaoIBSCBSGIBSCBSGTribRegular? extraGTribRegular = null;
        TributacaoIBSCBSGIBSCBSGTribCompraGov? extraGTribCompraGov = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vBC":
                    extraVBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "gIBSUF":
                    extraGIBSUF = await ReadTributacaoIBSCBSGIBSCBSGIBSUFAsync(ct).ConfigureAwait(false);
                    return true;
                case "gIBSMun":
                    extraGIBSMun = await ReadTributacaoIBSCBSGIBSCBSGIBSMunAsync(ct).ConfigureAwait(false);
                    return true;
                case "vIBS":
                    extraVIBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "gCBS":
                    extraGCBS = await ReadTributacaoIBSCBSGIBSCBSGCBSAsync(ct).ConfigureAwait(false);
                    return true;
                case "gTribRegular":
                    extraGTribRegular = await ReadTributacaoIBSCBSGIBSCBSGTribRegularAsync(ct).ConfigureAwait(false);
                    return true;
                case "gTribCompraGov":
                    extraGTribCompraGov = await ReadTributacaoIBSCBSGIBSCBSGTribCompraGovAsync(ct).ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBS
        {
            VBC = extraVBC,
            GIBSUF = extraGIBSUF,
            GIBSMun = extraGIBSMun,
            VIBS = extraVIBS,
            GCBS = extraGCBS,
            GTribRegular = extraGTribRegular,
            GTribCompraGov = extraGTribCompraGov
        };
    }
}
