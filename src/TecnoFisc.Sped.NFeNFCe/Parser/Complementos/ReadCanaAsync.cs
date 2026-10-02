// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<Cana> ReadCanaAsync(CancellationToken ct)
    {
        string? extraSafra = null;
        string? extraRef = null;
        List<CanaForDia>? extraForDia = null;
        decimal? extraQTotMes = null;
        decimal? extraQTotAnt = null;
        decimal? extraQTotGer = null;
        List<CanaDeduc>? extraDeduc = null;
        decimal? extraVFor = null;
        decimal? extraVTotDed = null;
        decimal? extraVLiqFor = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "safra": extraSafra = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "ref": extraRef = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "forDia": (extraForDia ??= []).Add(await ReadCanaForDiaAsync(ct).ConfigureAwait(false)); return true;
                case "qTotMes": extraQTotMes = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "qTotAnt": extraQTotAnt = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "qTotGer": extraQTotGer = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "deduc": (extraDeduc ??= []).Add(await ReadCanaDeducAsync(ct).ConfigureAwait(false)); return true;
                case "vFor": extraVFor = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vTotDed": extraVTotDed = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vLiqFor": extraVLiqFor = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new Cana
        {
            Safra = extraSafra,
            Ref = extraRef,
            ForDia = (IReadOnlyList<CanaForDia>?)extraForDia ?? [],
            QTotMes = extraQTotMes,
            QTotAnt = extraQTotAnt,
            QTotGer = extraQTotGer,
            Deduc = (IReadOnlyList<CanaDeduc>?)extraDeduc ?? [],
            VFor = extraVFor,
            VTotDed = extraVTotDed,
            VLiqFor = extraVLiqFor
        };
    }
}
