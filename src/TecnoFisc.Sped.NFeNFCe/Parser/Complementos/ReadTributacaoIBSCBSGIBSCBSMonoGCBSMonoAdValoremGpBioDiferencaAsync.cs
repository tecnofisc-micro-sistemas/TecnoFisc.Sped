// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGpBioDiferenca> ReadTributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGpBioDiferencaAsync(CancellationToken ct)
    {
        decimal? extraQBCBioComb = null;
        decimal? extraVCBSDiferenca = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "qBCBioComb": extraQBCBioComb = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vCBSDiferenca": extraVCBSDiferenca = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGpBioDiferenca
        {
            QBCBioComb = extraQBCBioComb,
            VCBSDiferenca = extraVCBSDiferenca
        };
    }
}
