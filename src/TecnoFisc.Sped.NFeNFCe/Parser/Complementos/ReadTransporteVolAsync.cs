// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TransporteVol> ReadTransporteVolAsync(CancellationToken ct)
    {
        string? extraQVol = null;
        string? extraEsp = null;
        string? extraMarca = null;
        string? extraNVol = null;
        decimal? extraPesoL = null;
        decimal? extraPesoB = null;
        List<TransporteVolLacres>? extraLacres = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "qVol": extraQVol = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "esp": extraEsp = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "marca": extraMarca = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "nVol": extraNVol = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "pesoL": extraPesoL = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pesoB": extraPesoB = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "lacres": (extraLacres ??= []).Add(await ReadTransporteVolLacresAsync(ct).ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TransporteVol
        {
            QVol = extraQVol,
            Esp = extraEsp,
            Marca = extraMarca,
            NVol = extraNVol,
            PesoL = extraPesoL,
            PesoB = extraPesoB,
            Lacres = (IReadOnlyList<TransporteVolLacres>?)extraLacres ?? []
        };
    }
}
