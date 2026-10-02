// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValorem> ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremAsync(CancellationToken ct)
    {
        TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGMonoPadrao? extraGMonoPadrao = null;
        TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGMonoReten? extraGMonoReten = null;
        TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGMonoRet? extraGMonoRet = null;
        TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGpBioDiferenca? extraGpBioDiferenca = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "gMonoPadrao": extraGMonoPadrao = await ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGMonoPadraoAsync(ct).ConfigureAwait(false); return true;
                case "gMonoReten": extraGMonoReten = await ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGMonoRetenAsync(ct).ConfigureAwait(false); return true;
                case "gMonoRet": extraGMonoRet = await ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGMonoRetAsync(ct).ConfigureAwait(false); return true;
                case "gpBioDiferenca": extraGpBioDiferenca = await ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGpBioDiferencaAsync(ct).ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValorem
        {
            GMonoPadrao = extraGMonoPadrao,
            GMonoReten = extraGMonoReten,
            GMonoRet = extraGMonoRet,
            GpBioDiferenca = extraGpBioDiferenca
        };
    }
}
