// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSGTribRegular> ReadTributacaoIBSCBSGIBSCBSGTribRegularAsync(CancellationToken ct)
    {
        string? extraCSTReg = null;
        string? extraCClassTribReg = null;
        decimal? extraPAliqEfetRegIBSUF = null;
        decimal? extraVTribRegIBSUF = null;
        decimal? extraPAliqEfetRegIBSMun = null;
        decimal? extraVTribRegIBSMun = null;
        decimal? extraPAliqEfetRegCBS = null;
        decimal? extraVTribRegCBS = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CSTReg": extraCSTReg = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "cClassTribReg": extraCClassTribReg = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "pAliqEfetRegIBSUF": extraPAliqEfetRegIBSUF = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vTribRegIBSUF": extraVTribRegIBSUF = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pAliqEfetRegIBSMun": extraPAliqEfetRegIBSMun = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vTribRegIBSMun": extraVTribRegIBSMun = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pAliqEfetRegCBS": extraPAliqEfetRegCBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vTribRegCBS": extraVTribRegCBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSGTribRegular
        {
            CSTReg = extraCSTReg,
            CClassTribReg = extraCClassTribReg,
            PAliqEfetRegIBSUF = extraPAliqEfetRegIBSUF,
            VTribRegIBSUF = extraVTribRegIBSUF,
            PAliqEfetRegIBSMun = extraPAliqEfetRegIBSMun,
            VTribRegIBSMun = extraVTribRegIBSMun,
            PAliqEfetRegCBS = extraPAliqEfetRegCBS,
            VTribRegCBS = extraVTribRegCBS
        };
    }
}
