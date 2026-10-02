using System.Globalization;
// Scaffolding revisÃƒÂ¡vel a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<Rastreabilidade> ReadRastreabilidadeAsync(CancellationToken ct)
    {
        string? extraNLote = null;
        decimal? extraQLote = null;
        DateOnly? extraDFab = null;
        DateOnly? extraDVal = null;
        string? extraCAgreg = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "nLote":
                    extraNLote = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "qLote":
                    extraQLote = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "dFab":
                    extraDFab = DateOnly.Parse(await _r.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture);
                    return true;
                case "dVal":
                    extraDVal = DateOnly.Parse(await _r.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture);
                    return true;
                case "cAgreg":
                    extraCAgreg = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new Rastreabilidade
        {
            NLote = extraNLote,
            QLote = extraQLote,
            DFab = extraDFab,
            DVal = extraDVal,
            CAgreg = extraCAgreg
        };
    }
}
