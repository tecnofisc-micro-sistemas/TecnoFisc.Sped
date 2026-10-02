// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<CombustivelEncerrante> ReadCombustivelEncerranteAsync(CancellationToken ct)
    {
        string? extraNBico = null;
        string? extraNBomba = null;
        string? extraNTanque = null;
        decimal? extraVEncIni = null;
        decimal? extraVEncFin = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "nBico":
                    extraNBico = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "nBomba":
                    extraNBomba = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "nTanque":
                    extraNTanque = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "vEncIni":
                    extraVEncIni = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vEncFin":
                    extraVEncFin = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new CombustivelEncerrante
        {
            NBico = extraNBico,
            NBomba = extraNBomba,
            NTanque = extraNTanque,
            VEncIni = extraVEncIni,
            VEncFin = extraVEncFin
        };
    }
}
