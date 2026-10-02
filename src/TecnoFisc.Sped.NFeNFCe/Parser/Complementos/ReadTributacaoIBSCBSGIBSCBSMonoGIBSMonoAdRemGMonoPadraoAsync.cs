// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoPadrao> ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoPadraoAsync(CancellationToken ct)
    {
        decimal? extraQBCMono = null;
        decimal? extraAdRemIBS = null;
        decimal? extraVIBSMono = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "qBCMono":
                    extraQBCMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "adRemIBS":
                    extraAdRemIBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vIBSMono":
                    extraVIBSMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoPadrao
        {
            QBCMono = extraQBCMono,
            AdRemIBS = extraAdRemIBS,
            VIBSMono = extraVIBSMono
        };
    }
}
