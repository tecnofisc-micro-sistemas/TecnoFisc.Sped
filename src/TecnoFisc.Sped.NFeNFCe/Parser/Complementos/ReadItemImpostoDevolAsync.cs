// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<ItemImpostoDevol> ReadItemImpostoDevolAsync(CancellationToken ct)
    {
        decimal? extraPDevol = null;
        ItemImpostoDevolIPI? extraIPI = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "pDevol":
                    extraPDevol = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "IPI":
                    extraIPI = await ReadItemImpostoDevolIPIAsync(ct).ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new ItemImpostoDevol
        {
            PDevol = extraPDevol,
            IPI = extraIPI
        };
    }
}
