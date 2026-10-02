// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSMono> ReadTributacaoIBSCBSGIBSCBSMonoAsync(CancellationToken ct)
    {
        TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRem? extraGIBSMonoAdRem = null;
        TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValorem? extraGIBSMonoAdValorem = null;
        TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRem? extraGCBSMonoAdRem = null;
        TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValorem? extraGCBSMonoAdValorem = null;
        decimal? extraVTotIBSMonoItem = null;
        decimal? extraVTotCBSMonoItem = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "gIBSMonoAdRem":
                    extraGIBSMonoAdRem = await ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemAsync(ct).ConfigureAwait(false);
                    return true;
                case "gIBSMonoAdValorem":
                    extraGIBSMonoAdValorem = await ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremAsync(ct).ConfigureAwait(false);
                    return true;
                case "gCBSMonoAdRem":
                    extraGCBSMonoAdRem = await ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemAsync(ct).ConfigureAwait(false);
                    return true;
                case "gCBSMonoAdValorem":
                    extraGCBSMonoAdValorem = await ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremAsync(ct).ConfigureAwait(false);
                    return true;
                case "vTotIBSMonoItem":
                    extraVTotIBSMonoItem = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vTotCBSMonoItem":
                    extraVTotCBSMonoItem = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSMono
        {
            GIBSMonoAdRem = extraGIBSMonoAdRem,
            GIBSMonoAdValorem = extraGIBSMonoAdValorem,
            GCBSMonoAdRem = extraGCBSMonoAdRem,
            GCBSMonoAdValorem = extraGCBSMonoAdValorem,
            VTotIBSMonoItem = extraVTotIBSMonoItem,
            VTotCBSMonoItem = extraVTotCBSMonoItem
        };
    }
}
