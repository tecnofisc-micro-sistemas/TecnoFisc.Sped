// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<IcmsUfDest> ReadIcmsUfDestAsync(CancellationToken ct)
    {
        decimal? extraVBCUFDest = null;
        decimal? extraVBCFCPUFDest = null;
        decimal? extraPFCPUFDest = null;
        decimal? extraPICMSUFDest = null;
        string? extraPICMSInter = null;
        decimal? extraPICMSInterPart = null;
        decimal? extraVFCPUFDest = null;
        decimal? extraVICMSUFDest = null;
        decimal? extraVICMSUFRemet = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vBCUFDest":
                    extraVBCUFDest = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vBCFCPUFDest":
                    extraVBCFCPUFDest = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "pFCPUFDest":
                    extraPFCPUFDest = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "pICMSUFDest":
                    extraPICMSUFDest = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "pICMSInter":
                    extraPICMSInter = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "pICMSInterPart":
                    extraPICMSInterPart = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vFCPUFDest":
                    extraVFCPUFDest = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vICMSUFDest":
                    extraVICMSUFDest = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vICMSUFRemet":
                    extraVICMSUFRemet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new IcmsUfDest
        {
            VBCUFDest = extraVBCUFDest,
            VBCFCPUFDest = extraVBCFCPUFDest,
            PFCPUFDest = extraPFCPUFDest,
            PICMSUFDest = extraPICMSUFDest,
            PICMSInter = extraPICMSInter,
            PICMSInterPart = extraPICMSInterPart,
            VFCPUFDest = extraVFCPUFDest,
            VICMSUFDest = extraVICMSUFDest,
            VICMSUFRemet = extraVICMSUFRemet
        };
    }
}
