// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValorem> ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremAsync(CancellationToken ct)
    {
        TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoPadrao? extraGMonoPadrao = null;
        TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoReten? extraGMonoReten = null;
        TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoRet? extraGMonoRet = null;
        TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGpBioDiferenca? extraGpBioDiferenca = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "gMonoPadrao":
                    extraGMonoPadrao = await ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoPadraoAsync(ct).ConfigureAwait(false);
                    return true;
                case "gMonoReten":
                    extraGMonoReten = await ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoRetenAsync(ct).ConfigureAwait(false);
                    return true;
                case "gMonoRet":
                    extraGMonoRet = await ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoRetAsync(ct).ConfigureAwait(false);
                    return true;
                case "gpBioDiferenca":
                    extraGpBioDiferenca = await ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGpBioDiferencaAsync(ct).ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValorem
        {
            GMonoPadrao = extraGMonoPadrao,
            GMonoReten = extraGMonoReten,
            GMonoRet = extraGMonoRet,
            GpBioDiferenca = extraGpBioDiferenca
        };
    }
}
