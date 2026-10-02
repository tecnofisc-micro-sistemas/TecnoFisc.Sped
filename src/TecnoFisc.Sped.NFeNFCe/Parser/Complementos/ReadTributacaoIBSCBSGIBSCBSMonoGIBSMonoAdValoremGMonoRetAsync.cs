// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoRet> ReadTributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoRetAsync(CancellationToken ct)
    {
        decimal? extraVIBSMonoRet = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vIBSMonoRet": extraVIBSMonoRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoRet
        {
            VIBSMonoRet = extraVIBSMonoRet
        };
    }
}
