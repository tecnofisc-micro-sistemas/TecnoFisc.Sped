// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSGIBSUF> ReadTributacaoIBSCBSGIBSCBSGIBSUFAsync(CancellationToken ct)
    {
        decimal? extraPIBSUF = null;
        TributacaoIBSCBSGIBSCBSGIBSUFGDif? extraGDif = null;
        TributacaoIBSCBSGIBSCBSGIBSUFGDevTrib? extraGDevTrib = null;
        TributacaoIBSCBSGIBSCBSGIBSUFGRed? extraGRed = null;
        decimal? extraVIBSUF = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "pIBSUF": extraPIBSUF = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "gDif": extraGDif = await ReadTributacaoIBSCBSGIBSCBSGIBSUFGDifAsync(ct).ConfigureAwait(false); return true;
                case "gDevTrib": extraGDevTrib = await ReadTributacaoIBSCBSGIBSCBSGIBSUFGDevTribAsync(ct).ConfigureAwait(false); return true;
                case "gRed": extraGRed = await ReadTributacaoIBSCBSGIBSCBSGIBSUFGRedAsync(ct).ConfigureAwait(false); return true;
                case "vIBSUF": extraVIBSUF = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSGIBSUF
        {
            PIBSUF = extraPIBSUF,
            GDif = extraGDif,
            GDevTrib = extraGDevTrib,
            GRed = extraGRed,
            VIBSUF = extraVIBSUF
        };
    }
}
