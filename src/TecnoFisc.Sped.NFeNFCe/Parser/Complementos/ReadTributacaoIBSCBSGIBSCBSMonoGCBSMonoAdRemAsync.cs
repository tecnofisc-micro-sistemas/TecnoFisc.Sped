// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRem> ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemAsync(CancellationToken ct)
    {
        TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoPadrao? extraGMonoPadrao = null;
        TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoReten? extraGMonoReten = null;
        TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoRet? extraGMonoRet = null;
        TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGpBioDiferenca? extraGpBioDiferenca = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "gMonoPadrao":
                    extraGMonoPadrao = await ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoPadraoAsync(ct).ConfigureAwait(false);
                    return true;
                case "gMonoReten":
                    extraGMonoReten = await ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoRetenAsync(ct).ConfigureAwait(false);
                    return true;
                case "gMonoRet":
                    extraGMonoRet = await ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoRetAsync(ct).ConfigureAwait(false);
                    return true;
                case "gpBioDiferenca":
                    extraGpBioDiferenca = await ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGpBioDiferencaAsync(ct).ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRem
        {
            GMonoPadrao = extraGMonoPadrao,
            GMonoReten = extraGMonoReten,
            GMonoRet = extraGMonoRet,
            GpBioDiferenca = extraGpBioDiferenca
        };
    }
}
