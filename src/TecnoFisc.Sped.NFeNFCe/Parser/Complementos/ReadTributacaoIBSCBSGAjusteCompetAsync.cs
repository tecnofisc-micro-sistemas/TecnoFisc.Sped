// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGAjusteCompet> ReadTributacaoIBSCBSGAjusteCompetAsync(CancellationToken ct)
    {
        string? extraCompetApur = null;
        decimal? extraVIBS = null;
        decimal? extraVCBS = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "competApur": extraCompetApur = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "vIBS": extraVIBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vCBS": extraVCBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGAjusteCompet
        {
            CompetApur = extraCompetApur,
            VIBS = extraVIBS,
            VCBS = extraVCBS
        };
    }
}
