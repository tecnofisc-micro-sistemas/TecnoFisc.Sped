// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSGIBSMun> ReadTributacaoIBSCBSGIBSCBSGIBSMunAsync(CancellationToken ct)
    {
        decimal? extraPIBSMun = null;
        TributacaoIBSCBSGIBSCBSGIBSMunGDif? extraGDif = null;
        TributacaoIBSCBSGIBSCBSGIBSMunGDevTrib? extraGDevTrib = null;
        TributacaoIBSCBSGIBSCBSGIBSMunGRed? extraGRed = null;
        decimal? extraVIBSMun = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "pIBSMun":
                    extraPIBSMun = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "gDif":
                    extraGDif = await ReadTributacaoIBSCBSGIBSCBSGIBSMunGDifAsync(ct).ConfigureAwait(false);
                    return true;
                case "gDevTrib":
                    extraGDevTrib = await ReadTributacaoIBSCBSGIBSCBSGIBSMunGDevTribAsync(ct).ConfigureAwait(false);
                    return true;
                case "gRed":
                    extraGRed = await ReadTributacaoIBSCBSGIBSCBSGIBSMunGRedAsync(ct).ConfigureAwait(false);
                    return true;
                case "vIBSMun":
                    extraVIBSMun = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSGIBSMun
        {
            PIBSMun = extraPIBSMun,
            GDif = extraGDif,
            GDevTrib = extraGDevTrib,
            GRed = extraGRed,
            VIBSMun = extraVIBSMun
        };
    }
}
