// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSGIBSUFGDevTrib> ReadTributacaoIBSCBSGIBSCBSGIBSUFGDevTribAsync(CancellationToken ct)
    {
        decimal? extraPDevTrib = null;
        decimal? extraVDevTrib = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "pDevTrib":
                    extraPDevTrib = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vDevTrib":
                    extraVDevTrib = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSGIBSUFGDevTrib
        {
            PDevTrib = extraPDevTrib,
            VDevTrib = extraVDevTrib
        };
    }
}
