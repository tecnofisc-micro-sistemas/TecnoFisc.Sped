// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<ProdutoInfProdEmb> ReadProdutoInfProdEmbAsync(CancellationToken ct)
    {
        string? extraXEmb = null;
        decimal? extraQVolEmb = null;
        string? extraUEmb = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "xEmb":
                    extraXEmb = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "qVolEmb":
                    extraQVolEmb = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "uEmb":
                    extraUEmb = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new ProdutoInfProdEmb
        {
            XEmb = extraXEmb,
            QVolEmb = extraQVolEmb,
            UEmb = extraUEmb
        };
    }
}
