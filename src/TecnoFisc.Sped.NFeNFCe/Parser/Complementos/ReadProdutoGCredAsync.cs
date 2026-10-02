// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<ProdutoGCred> ReadProdutoGCredAsync(CancellationToken ct)
    {
        string? extraCCredPresumido = null;
        decimal? extraPCredPresumido = null;
        decimal? extraVCredPresumido = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "cCredPresumido":
                    extraCCredPresumido = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "pCredPresumido":
                    extraPCredPresumido = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vCredPresumido":
                    extraVCredPresumido = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new ProdutoGCred
        {
            CCredPresumido = extraCCredPresumido,
            PCredPresumido = extraPCredPresumido,
            VCredPresumido = extraVCredPresumido
        };
    }
}
