// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGMonoPadrao> ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGMonoPadraoAsync(CancellationToken ct)
    {
        decimal? extraVBCMono = null;
        decimal? extraPAliqMonoCBS = null;
        decimal? extraVCBSMono = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vBCMono": extraVBCMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pAliqMonoCBS": extraPAliqMonoCBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vCBSMono": extraVCBSMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGMonoPadrao
        {
            VBCMono = extraVBCMono,
            PAliqMonoCBS = extraPAliqMonoCBS,
            VCBSMono = extraVCBSMono
        };
    }
}
