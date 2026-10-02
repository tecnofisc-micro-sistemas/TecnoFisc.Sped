// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<InfNFeInfSolicNFF> ReadInfNFeInfSolicNFFAsync(CancellationToken ct)
    {
        string? extraXSolic = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "xSolic": extraXSolic = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new InfNFeInfSolicNFF
        {
            XSolic = extraXSolic
        };
    }
}
