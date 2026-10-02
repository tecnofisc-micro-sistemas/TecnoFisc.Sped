// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.
using System.Globalization;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<Icms15> ReadIcms15Async(CancellationToken ct)
    {
        string? extraOrig = null;
        string? extraCST = null;
        decimal? extraQBCMono = null;
        decimal? extraAdRemICMS = null;
        decimal? extraVICMSMono = null;
        decimal? extraQBCMonoReten = null;
        decimal? extraAdRemICMSReten = null;
        decimal? extraVICMSMonoReten = null;
        decimal? extraPRedAdRem = null;
        string? extraMotRedAdRem = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": extraOrig = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CST": extraCST = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "qBCMono": extraQBCMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "adRemICMS": extraAdRemICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSMono": extraVICMSMono = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "qBCMonoReten": extraQBCMonoReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "adRemICMSReten": extraAdRemICMSReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSMonoReten": extraVICMSMonoReten = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedAdRem": extraPRedAdRem = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "motRedAdRem": extraMotRedAdRem = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new Icms15
        {
            Orig = (OrigemMercadoria)int.Parse(extraOrig ?? "0", CultureInfo.InvariantCulture),
            CST = extraCST,
            QBCMono = extraQBCMono,
            AdRemICMS = extraAdRemICMS,
            VICMSMono = extraVICMSMono,
            QBCMonoReten = extraQBCMonoReten,
            AdRemICMSReten = extraAdRemICMSReten,
            VICMSMonoReten = extraVICMSMonoReten,
            PRedAdRem = extraPRedAdRem,
            MotRedAdRem = extraMotRedAdRem
        };
    }
}
