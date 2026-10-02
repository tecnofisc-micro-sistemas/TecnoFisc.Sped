// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<CobrancaFat> ReadCobrancaFatAsync(CancellationToken ct)
    {
        string? extraNFat = null;
        decimal? extraVOrig = null;
        decimal? extraVDesc = null;
        decimal? extraVLiq = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "nFat":
                    extraNFat = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "vOrig":
                    extraVOrig = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vDesc":
                    extraVDesc = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vLiq":
                    extraVLiq = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new CobrancaFat
        {
            NFat = extraNFat,
            VOrig = extraVOrig,
            VDesc = extraVDesc,
            VLiq = extraVLiq
        };
    }
}
