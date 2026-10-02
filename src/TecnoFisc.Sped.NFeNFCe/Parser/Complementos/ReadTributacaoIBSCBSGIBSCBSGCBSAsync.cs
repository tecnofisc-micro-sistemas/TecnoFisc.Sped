// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSGCBS> ReadTributacaoIBSCBSGIBSCBSGCBSAsync(CancellationToken ct)
    {
        decimal? extraPCBS = null;
        TributacaoIBSCBSGIBSCBSGCBSGDif? extraGDif = null;
        TributacaoIBSCBSGIBSCBSGCBSGDevTrib? extraGDevTrib = null;
        TributacaoIBSCBSGIBSCBSGCBSGRed? extraGRed = null;
        TributacaoIBSCBSGIBSCBSGCBSGALCZFMCBS? extraGALCZFMCBS = null;
        decimal? extraVCBS = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "pCBS": extraPCBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "gDif": extraGDif = await ReadTributacaoIBSCBSGIBSCBSGCBSGDifAsync(ct).ConfigureAwait(false); return true;
                case "gDevTrib": extraGDevTrib = await ReadTributacaoIBSCBSGIBSCBSGCBSGDevTribAsync(ct).ConfigureAwait(false); return true;
                case "gRed": extraGRed = await ReadTributacaoIBSCBSGIBSCBSGCBSGRedAsync(ct).ConfigureAwait(false); return true;
                case "gALCZFMCBS": extraGALCZFMCBS = await ReadTributacaoIBSCBSGIBSCBSGCBSGALCZFMCBSAsync(ct).ConfigureAwait(false); return true;
                case "vCBS": extraVCBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSGCBS
        {
            PCBS = extraPCBS,
            GDif = extraGDif,
            GDevTrib = extraGDevTrib,
            GRed = extraGRed,
            GALCZFMCBS = extraGALCZFMCBS,
            VCBS = extraVCBS
        };
    }
}
