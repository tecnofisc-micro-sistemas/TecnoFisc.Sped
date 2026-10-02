// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<ItemObsItem> ReadItemObsItemAsync(CancellationToken ct)
    {
        ItemObsItemObsCont? extraObsCont = null;
        ItemObsItemObsFisco? extraObsFisco = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "obsCont": extraObsCont = await ReadItemObsItemObsContAsync(ct).ConfigureAwait(false); return true;
                case "obsFisco": extraObsFisco = await ReadItemObsItemObsFiscoAsync(ct).ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new ItemObsItem
        {
            ObsCont = extraObsCont,
            ObsFisco = extraObsFisco
        };
    }
}
