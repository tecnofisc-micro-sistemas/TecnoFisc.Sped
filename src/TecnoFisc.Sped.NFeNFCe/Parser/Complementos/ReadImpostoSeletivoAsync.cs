// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<ImpostoSeletivo> ReadImpostoSeletivoAsync(CancellationToken ct)
    {
        string? extraCSTIS = null;
        string? extraCClassTribIS = null;
        decimal? extraVBCIS = null;
        decimal? extraPIS = null;
        decimal? extraAdRemIS = null;
        string? extraUTrib = null;
        decimal? extraQTrib = null;
        decimal? extraVIS = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CSTIS": extraCSTIS = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cClassTribIS": extraCClassTribIS = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "vBCIS": extraVBCIS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pIS": extraPIS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "adRemIS": extraAdRemIS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "uTrib": extraUTrib = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "qTrib": extraQTrib = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vIS": extraVIS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new ImpostoSeletivo
        {
            CSTIS = extraCSTIS,
            CClassTribIS = extraCClassTribIS,
            VBCIS = extraVBCIS,
            PIS = extraPIS,
            AdRemIS = extraAdRemIS,
            UTrib = extraUTrib,
            QTrib = extraQTrib,
            VIS = extraVIS
        };
    }
}
