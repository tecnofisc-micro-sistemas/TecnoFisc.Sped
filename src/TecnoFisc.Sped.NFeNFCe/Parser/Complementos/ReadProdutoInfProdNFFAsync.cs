// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<ProdutoInfProdNFF> ReadProdutoInfProdNFFAsync(CancellationToken ct)
    {
        string? extraCProdFisco = null;
        string? extraCOperNFF = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "cProdFisco": extraCProdFisco = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cOperNFF": extraCOperNFF = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new ProdutoInfProdNFF
        {
            CProdFisco = extraCProdFisco,
            COperNFF = extraCOperNFF
        };
    }
}
