// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGCredPresIBSZFM> ReadTributacaoIBSCBSGCredPresIBSZFMAsync(CancellationToken ct)
    {
        string? extraCompetApur = null;
        string? extraTpCredPresIBSZFM = null;
        decimal? extraVCredPresIBSZFM = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "competApur": extraCompetApur = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "tpCredPresIBSZFM": extraTpCredPresIBSZFM = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "vCredPresIBSZFM": extraVCredPresIBSZFM = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGCredPresIBSZFM
        {
            CompetApur = extraCompetApur,
            TpCredPresIBSZFM = extraTpCredPresIBSZFM,
            VCredPresIBSZFM = extraVCredPresIBSZFM
        };
    }
}
