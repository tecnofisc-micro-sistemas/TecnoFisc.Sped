// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBS> ReadTributacaoIBSCBSAsync(CancellationToken ct)
    {
        string? extraCST = null;
        string? extraCClassTrib = null;
        string? extraIndDoacao = null;
        TributacaoIBSCBSGIBSCBS? extraGIBSCBS = null;
        TributacaoIBSCBSGIBSCBSMono? extraGIBSCBSMono = null;
        TributacaoIBSCBSGTransfCred? extraGTransfCred = null;
        TributacaoIBSCBSGAjusteCompet? extraGAjusteCompet = null;
        TributacaoIBSCBSGEstornoCred? extraGEstornoCred = null;
        TributacaoIBSCBSGCredPresOper? extraGCredPresOper = null;
        TributacaoIBSCBSGCredPresIBSZFM? extraGCredPresIBSZFM = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CST": extraCST = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cClassTrib": extraCClassTrib = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "indDoacao": extraIndDoacao = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "gIBSCBS": extraGIBSCBS = await ReadTributacaoIBSCBSGIBSCBSAsync(ct).ConfigureAwait(false); return true;
                case "gIBSCBSMono": extraGIBSCBSMono = await ReadTributacaoIBSCBSGIBSCBSMonoAsync(ct).ConfigureAwait(false); return true;
                case "gTransfCred": extraGTransfCred = await ReadTributacaoIBSCBSGTransfCredAsync(ct).ConfigureAwait(false); return true;
                case "gAjusteCompet": extraGAjusteCompet = await ReadTributacaoIBSCBSGAjusteCompetAsync(ct).ConfigureAwait(false); return true;
                case "gEstornoCred": extraGEstornoCred = await ReadTributacaoIBSCBSGEstornoCredAsync(ct).ConfigureAwait(false); return true;
                case "gCredPresOper": extraGCredPresOper = await ReadTributacaoIBSCBSGCredPresOperAsync(ct).ConfigureAwait(false); return true;
                case "gCredPresIBSZFM": extraGCredPresIBSZFM = await ReadTributacaoIBSCBSGCredPresIBSZFMAsync(ct).ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBS
        {
            CST = extraCST,
            CClassTrib = extraCClassTrib,
            IndDoacao = extraIndDoacao,
            GIBSCBS = extraGIBSCBS,
            GIBSCBSMono = extraGIBSCBSMono,
            GTransfCred = extraGTransfCred,
            GAjusteCompet = extraGAjusteCompet,
            GEstornoCred = extraGEstornoCred,
            GCredPresOper = extraGCredPresOper,
            GCredPresIBSZFM = extraGCredPresIBSZFM
        };
    }
}
