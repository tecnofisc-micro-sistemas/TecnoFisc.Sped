using System.Globalization;
// Scaffolding revisÃ¡vel a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TotalIssqn> ReadTotalIssqnAsync(CancellationToken ct)
    {
        decimal? extraVServ = null;
        decimal? extraVBC = null;
        decimal? extraVISS = null;
        decimal? extraVPIS = null;
        decimal? extraVCOFINS = null;
        DateOnly? extraDCompet = null;
        decimal? extraVDeducao = null;
        decimal? extraVOutro = null;
        decimal? extraVDescIncond = null;
        decimal? extraVDescCond = null;
        decimal? extraVISSRet = null;
        string? extraCRegTrib = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vServ":
                    extraVServ = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vBC":
                    extraVBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vISS":
                    extraVISS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vPIS":
                    extraVPIS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vCOFINS":
                    extraVCOFINS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "dCompet":
                    extraDCompet = DateOnly.Parse(await _r.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture);
                    return true;
                case "vDeducao":
                    extraVDeducao = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vOutro":
                    extraVOutro = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vDescIncond":
                    extraVDescIncond = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vDescCond":
                    extraVDescCond = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vISSRet":
                    extraVISSRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "cRegTrib":
                    extraCRegTrib = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TotalIssqn
        {
            VServ = extraVServ,
            VBC = extraVBC,
            VISS = extraVISS,
            VPIS = extraVPIS,
            VCOFINS = extraVCOFINS,
            DCompet = extraDCompet,
            VDeducao = extraVDeducao,
            VOutro = extraVOutro,
            VDescIncond = extraVDescIncond,
            VDescCond = extraVDescCond,
            VISSRet = extraVISSRet,
            CRegTrib = extraCRegTrib
        };
    }
}
