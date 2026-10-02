// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGTransfCred> ReadTributacaoIBSCBSGTransfCredAsync(CancellationToken ct)
    {
        decimal? extraVIBS = null;
        decimal? extraVCBS = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vIBS":
                    extraVIBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vCBS":
                    extraVCBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGTransfCred
        {
            VIBS = extraVIBS,
            VCBS = extraVCBS
        };
    }
}
