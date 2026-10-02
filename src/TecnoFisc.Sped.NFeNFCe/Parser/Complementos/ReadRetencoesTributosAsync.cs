// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<RetencoesTributos> ReadRetencoesTributosAsync(CancellationToken ct)
    {
        decimal? extraVRetPIS = null;
        decimal? extraVRetCOFINS = null;
        decimal? extraVRetCSLL = null;
        decimal? extraVBCIRRF = null;
        decimal? extraVIRRF = null;
        decimal? extraVBCRetPrev = null;
        decimal? extraVRetPrev = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vRetPIS": extraVRetPIS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vRetCOFINS": extraVRetCOFINS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vRetCSLL": extraVRetCSLL = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCIRRF": extraVBCIRRF = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vIRRF": extraVIRRF = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCRetPrev": extraVBCRetPrev = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vRetPrev": extraVRetPrev = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new RetencoesTributos
        {
            VRetPIS = extraVRetPIS,
            VRetCOFINS = extraVRetCOFINS,
            VRetCSLL = extraVRetCSLL,
            VBCIRRF = extraVBCIRRF,
            VIRRF = extraVIRRF,
            VBCRetPrev = extraVBCRetPrev,
            VRetPrev = extraVRetPrev
        };
    }
}
