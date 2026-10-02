// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<CombustivelOrigComb> ReadCombustivelOrigCombAsync(CancellationToken ct)
    {
        string? extraIndImport = null;
        string? extraCUFOrig = null;
        decimal? extraPOrig = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "indImport":
                    extraIndImport = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "cUFOrig":
                    extraCUFOrig = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "pOrig":
                    extraPOrig = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new CombustivelOrigComb
        {
            IndImport = extraIndImport,
            CUFOrig = extraCUFOrig,
            POrig = extraPOrig
        };
    }
}
