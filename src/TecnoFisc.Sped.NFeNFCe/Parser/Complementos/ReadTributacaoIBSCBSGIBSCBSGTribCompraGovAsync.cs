// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSGTribCompraGov> ReadTributacaoIBSCBSGIBSCBSGTribCompraGovAsync(CancellationToken ct)
    {
        decimal? extraPAliqIBSUF = null;
        decimal? extraVTribIBSUF = null;
        decimal? extraPAliqIBSMun = null;
        decimal? extraVTribIBSMun = null;
        decimal? extraPAliqCBS = null;
        decimal? extraVTribCBS = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "pAliqIBSUF":
                    extraPAliqIBSUF = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vTribIBSUF":
                    extraVTribIBSUF = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "pAliqIBSMun":
                    extraPAliqIBSMun = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vTribIBSMun":
                    extraVTribIBSMun = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "pAliqCBS":
                    extraPAliqCBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vTribCBS":
                    extraVTribCBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSGTribCompraGov
        {
            PAliqIBSUF = extraPAliqIBSUF,
            VTribIBSUF = extraVTribIBSUF,
            PAliqIBSMun = extraPAliqIBSMun,
            VTribIBSMun = extraVTribIBSMun,
            PAliqCBS = extraPAliqCBS,
            VTribCBS = extraVTribCBS
        };
    }
}
