// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRem> ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemAsync(CancellationToken ct)
    {
        TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoPadrao? extraGMonoPadrao = null;
        TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoReten? extraGMonoReten = null;
        TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoRet? extraGMonoRet = null;
        TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGpBioDiferenca? extraGpBioDiferenca = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "gMonoPadrao":
                    extraGMonoPadrao = await ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoPadraoAsync(ct).ConfigureAwait(false);
                    return true;
                case "gMonoReten":
                    extraGMonoReten = await ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoRetenAsync(ct).ConfigureAwait(false);
                    return true;
                case "gMonoRet":
                    extraGMonoRet = await ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoRetAsync(ct).ConfigureAwait(false);
                    return true;
                case "gpBioDiferenca":
                    extraGpBioDiferenca = await ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGpBioDiferencaAsync(ct).ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRem
        {
            GMonoPadrao = extraGMonoPadrao,
            GMonoReten = extraGMonoReten,
            GMonoRet = extraGMonoRet,
            GpBioDiferenca = extraGpBioDiferenca
        };
    }
}
