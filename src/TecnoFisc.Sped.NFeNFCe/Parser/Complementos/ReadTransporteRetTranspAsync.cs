// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TransporteRetTransp> ReadTransporteRetTranspAsync(CancellationToken ct)
    {
        decimal? extraVServ = null;
        decimal? extraVBCRet = null;
        decimal? extraPICMSRet = null;
        decimal? extraVICMSRet = null;
        string? extraCFOP = null;
        string? extraCMunFG = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vServ": extraVServ = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCRet": extraVBCRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMSRet": extraPICMSRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSRet": extraVICMSRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CFOP": extraCFOP = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cMunFG": extraCMunFG = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TransporteRetTransp
        {
            VServ = extraVServ,
            VBCRet = extraVBCRet,
            PICMSRet = extraPICMSRet,
            VICMSRet = extraVICMSRet,
            CFOP = extraCFOP,
            CMunFG = extraCMunFG
        };
    }
}
