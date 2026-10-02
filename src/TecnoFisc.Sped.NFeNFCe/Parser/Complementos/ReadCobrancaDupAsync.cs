using System.Globalization;
// Scaffolding revisÃ¡vel a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<CobrancaDup> ReadCobrancaDupAsync(CancellationToken ct)
    {
        string? extraNDup = null;
        DateOnly? extraDVenc = null;
        decimal? extraVDup = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "nDup": extraNDup = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "dVenc": extraDVenc = DateOnly.Parse(await _r.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture); return true;
                case "vDup": extraVDup = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new CobrancaDup
        {
            NDup = extraNDup,
            DVenc = extraDVenc,
            VDup = extraVDup
        };
    }
}
