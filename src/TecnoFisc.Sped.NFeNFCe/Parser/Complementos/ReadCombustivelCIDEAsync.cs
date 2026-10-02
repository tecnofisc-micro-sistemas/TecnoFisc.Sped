// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<CombustivelCIDE> ReadCombustivelCIDEAsync(CancellationToken ct)
    {
        decimal? extraQBCProd = null;
        decimal? extraVAliqProd = null;
        decimal? extraVCIDE = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "qBCProd":
                    extraQBCProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vAliqProd":
                    extraVAliqProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vCIDE":
                    extraVCIDE = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new CombustivelCIDE
        {
            QBCProd = extraQBCProd,
            VAliqProd = extraVAliqProd,
            VCIDE = extraVCIDE
        };
    }
}
