// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.
using System.Globalization;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<Icms61> ReadIcms61Async(CancellationToken ct)
    {
        string? extraOrig = null;
        string? extraCST = null;
        decimal? extraQBCMonoRet = null;
        decimal? extraAdRemICMSRet = null;
        decimal? extraVICMSMonoRet = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": extraOrig = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CST": extraCST = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "qBCMonoRet": extraQBCMonoRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "adRemICMSRet": extraAdRemICMSRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSMonoRet": extraVICMSMonoRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new Icms61
        {
            Orig = (OrigemMercadoria)int.Parse(extraOrig ?? "0", CultureInfo.InvariantCulture),
            CST = extraCST,
            QBCMonoRet = extraQBCMonoRet,
            AdRemICMSRet = extraAdRemICMSRet,
            VICMSMonoRet = extraVICMSMonoRet
        };
    }
}
