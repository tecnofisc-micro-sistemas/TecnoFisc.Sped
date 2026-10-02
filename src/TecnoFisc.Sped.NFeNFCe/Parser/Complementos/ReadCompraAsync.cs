// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<Compra> ReadCompraAsync(CancellationToken ct)
    {
        string? extraXNEmp = null;
        string? extraXPed = null;
        string? extraXCont = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "xNEmp":
                    extraXNEmp = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "xPed":
                    extraXPed = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "xCont":
                    extraXCont = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new Compra
        {
            XNEmp = extraXNEmp,
            XPed = extraXPed,
            XCont = extraXCont
        };
    }
}
