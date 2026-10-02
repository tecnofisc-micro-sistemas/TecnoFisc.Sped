// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<CanaDeduc> ReadCanaDeducAsync(CancellationToken ct)
    {
        string? extraXDed = null;
        decimal? extraVDed = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "xDed": extraXDed = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "vDed": extraVDed = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new CanaDeduc
        {
            XDed = extraXDed,
            VDed = extraVDed
        };
    }
}
