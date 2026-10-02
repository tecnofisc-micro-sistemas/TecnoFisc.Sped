// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<Pagamento> ReadPagamentoAsync(CancellationToken ct)
    {
        List<PagamentoDetPag>? extraDetPag = null;
        decimal? extraVTroco = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "detPag":
                    (extraDetPag ??= []).Add(await ReadPagamentoDetPagAsync(ct).ConfigureAwait(false));
                    return true;
                case "vTroco":
                    extraVTroco = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new Pagamento
        {
            DetPag = (IReadOnlyList<PagamentoDetPag>?)extraDetPag ?? [],
            VTroco = extraVTroco
        };
    }
}
