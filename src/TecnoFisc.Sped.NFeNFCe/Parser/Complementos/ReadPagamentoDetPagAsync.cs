using System.Globalization;
// Scaffolding revisÃ¡vel a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<PagamentoDetPag> ReadPagamentoDetPagAsync(CancellationToken ct)
    {
        string? extraIndPag = null;
        string? extraTPag = null;
        string? extraXPag = null;
        decimal? extraVPag = null;
        DateOnly? extraDPag = null;
        string? extraCNPJPag = null;
        string? extraUFPag = null;
        PagamentoDetPagCard? extraCard = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "indPag":
                    extraIndPag = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "tPag":
                    extraTPag = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "xPag":
                    extraXPag = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "vPag":
                    extraVPag = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "dPag":
                    extraDPag = DateOnly.Parse(await _r.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture);
                    return true;
                case "CNPJPag":
                    extraCNPJPag = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "UFPag":
                    extraUFPag = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "card":
                    extraCard = await ReadPagamentoDetPagCardAsync(ct).ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new PagamentoDetPag
        {
            IndPag = extraIndPag,
            TPag = extraTPag,
            XPag = extraXPag,
            VPag = extraVPag,
            DPag = extraDPag,
            CNPJPag = extraCNPJPag,
            UFPag = extraUFPag,
            Card = extraCard
        };
    }
}
