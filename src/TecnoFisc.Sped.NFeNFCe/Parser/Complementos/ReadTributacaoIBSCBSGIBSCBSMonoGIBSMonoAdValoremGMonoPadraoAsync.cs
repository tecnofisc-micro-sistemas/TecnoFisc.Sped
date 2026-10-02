// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoPadrao> ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoPadraoAsync(CancellationToken ct)
    {
        decimal? extraVBCMono = null;
        decimal? extraPAliqMonoUF = null;
        decimal? extraVIBSMonoUF = null;
        decimal? extraPAliqMonoMun = null;
        decimal? extraVIBSMonoMun = null;
        decimal? extraVIBSMono = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vBCMono":
                    extraVBCMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "pAliqMonoUF":
                    extraPAliqMonoUF = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vIBSMonoUF":
                    extraVIBSMonoUF = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "pAliqMonoMun":
                    extraPAliqMonoMun = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vIBSMonoMun":
                    extraVIBSMonoMun = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vIBSMono":
                    extraVIBSMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoPadrao
        {
            VBCMono = extraVBCMono,
            PAliqMonoUF = extraPAliqMonoUF,
            VIBSMonoUF = extraVIBSMonoUF,
            PAliqMonoMun = extraPAliqMonoMun,
            VIBSMonoMun = extraVIBSMonoMun,
            VIBSMono = extraVIBSMono
        };
    }
}
