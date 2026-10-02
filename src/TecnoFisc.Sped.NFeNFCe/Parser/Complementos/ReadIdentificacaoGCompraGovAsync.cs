// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<IdentificacaoGCompraGov> ReadIdentificacaoGCompraGovAsync(CancellationToken ct)
    {
        string? extraTpEnteGov = null;
        decimal? extraPRedutor = null;
        string? extraTpOperGov = null;
        List<string>? extraRefDFeAnt = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "tpEnteGov":
                    extraTpEnteGov = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "pRedutor":
                    extraPRedutor = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "tpOperGov":
                    extraTpOperGov = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "refDFeAnt":
                    (extraRefDFeAnt ??= []).Add(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new IdentificacaoGCompraGov
        {
            TpEnteGov = extraTpEnteGov,
            PRedutor = extraPRedutor,
            TpOperGov = extraTpOperGov,
            RefDFeAnt = (IReadOnlyList<string>?)extraRefDFeAnt ?? []
        };
    }
}
