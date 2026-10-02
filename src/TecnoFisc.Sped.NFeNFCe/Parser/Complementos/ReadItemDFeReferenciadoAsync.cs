// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<ItemDFeReferenciado> ReadItemDFeReferenciadoAsync(CancellationToken ct)
    {
        string? extraChaveAcesso = null;
        string? extraNItem = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "chaveAcesso": extraChaveAcesso = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "nItem": extraNItem = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new ItemDFeReferenciado
        {
            ChaveAcesso = extraChaveAcesso,
            NItem = extraNItem
        };
    }
}
