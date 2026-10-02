// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoPadrao> ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoPadraoAsync(CancellationToken ct)
    {
        decimal? extraQBCMono = null;
        decimal? extraAdRemCBS = null;
        decimal? extraVCBSMono = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "qBCMono":
                    extraQBCMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "adRemCBS":
                    extraAdRemCBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vCBSMono":
                    extraVCBSMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoPadrao
        {
            QBCMono = extraQBCMono,
            AdRemCBS = extraAdRemCBS,
            VCBSMono = extraVCBSMono
        };
    }
}
