// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGEstornoCred> ReadTributacaoIBSCBSGEstornoCredAsync(CancellationToken ct)
    {
        decimal? extraVIBSEstCred = null;
        decimal? extraVCBSEstCred = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vIBSEstCred": extraVIBSEstCred = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vCBSEstCred": extraVCBSEstCred = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGEstornoCred
        {
            VIBSEstCred = extraVIBSEstCred,
            VCBSEstCred = extraVCBSEstCred
        };
    }
}
