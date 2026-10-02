// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGCredPresOper> ReadTributacaoIBSCBSGCredPresOperAsync(CancellationToken ct)
    {
        decimal? extraVBCCredPres = null;
        string? extraCCredPres = null;
        TributacaoIBSCBSGCredPresOperGIBSCredPres? extraGIBSCredPres = null;
        TributacaoIBSCBSGCredPresOperGCBSCredPres? extraGCBSCredPres = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vBCCredPres": extraVBCCredPres = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "cCredPres": extraCCredPres = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "gIBSCredPres": extraGIBSCredPres = await ReadTributacaoIBSCBSGCredPresOperGIBSCredPresAsync(ct).ConfigureAwait(false); return true;
                case "gCBSCredPres": extraGCBSCredPres = await ReadTributacaoIBSCBSGCredPresOperGCBSCredPresAsync(ct).ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGCredPresOper
        {
            VBCCredPres = extraVBCCredPres,
            CCredPres = extraCCredPres,
            GIBSCredPres = extraGIBSCredPres,
            GCBSCredPres = extraGCBSCredPres
        };
    }
}
