// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.
using System.Globalization;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<Icms53> ReadIcms53Async(CancellationToken ct)
    {
        string? extraOrig = null;
        string? extraCST = null;
        decimal? extraQBCMono = null;
        decimal? extraAdRemICMS = null;
        decimal? extraVICMSMonoOp = null;
        decimal? extraPDif = null;
        decimal? extraVICMSMonoDif = null;
        decimal? extraVICMSMono = null;
        decimal? extraQBCMonoDif = null;
        decimal? extraAdRemICMSDif = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig":
                    extraOrig = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "CST":
                    extraCST = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "qBCMono":
                    extraQBCMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "adRemICMS":
                    extraAdRemICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vICMSMonoOp":
                    extraVICMSMonoOp = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "pDif":
                    extraPDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vICMSMonoDif":
                    extraVICMSMonoDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vICMSMono":
                    extraVICMSMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "qBCMonoDif":
                    extraQBCMonoDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "adRemICMSDif":
                    extraAdRemICMSDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new Icms53
        {
            Orig = (OrigemMercadoria)int.Parse(extraOrig ?? "0", CultureInfo.InvariantCulture),
            CST = extraCST,
            QBCMono = extraQBCMono,
            AdRemICMS = extraAdRemICMS,
            VICMSMonoOp = extraVICMSMonoOp,
            PDif = extraPDif,
            VICMSMonoDif = extraVICMSMonoDif,
            VICMSMono = extraVICMSMono,
            QBCMonoDif = extraQBCMonoDif,
            AdRemICMSDif = extraAdRemICMSDif
        };
    }
}
