// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<ItemObsItemObsCont> ReadItemObsItemObsContAsync(CancellationToken ct)
    {
        string? extraXTexto = null;
        string? extraXCampo = _r.GetAttribute("xCampo");
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "xTexto":
                    extraXTexto = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "xCampo":
                    extraXCampo = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new ItemObsItemObsCont
        {
            XTexto = extraXTexto,
            XCampo = extraXCampo
        };
    }
}
