// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<PagamentoDetPagCard> ReadPagamentoDetPagCardAsync(CancellationToken ct)
    {
        string? extraTpIntegra = null;
        string? extraCNPJ = null;
        string? extraTBand = null;
        string? extraCAut = null;
        string? extraCNPJReceb = null;
        string? extraIdTermPag = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "tpIntegra": extraTpIntegra = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CNPJ": extraCNPJ = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "tBand": extraTBand = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cAut": extraCAut = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CNPJReceb": extraCNPJReceb = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "idTermPag": extraIdTermPag = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new PagamentoDetPagCard
        {
            TpIntegra = extraTpIntegra,
            CNPJ = extraCNPJ,
            TBand = extraTBand,
            CAut = extraCAut,
            CNPJReceb = extraCNPJReceb,
            IdTermPag = extraIdTermPag
        };
    }
}
