// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSGIBSMunGRed> ReadTributacaoIBSCBSGIBSCBSGIBSMunGRedAsync(CancellationToken ct)
    {
        decimal? extraPRedAliq = null;
        decimal? extraPAliqEfet = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "pRedAliq":
                    extraPRedAliq = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "pAliqEfet":
                    extraPAliqEfet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSGIBSMunGRed
        {
            PRedAliq = extraPRedAliq,
            PAliqEfet = extraPAliqEfet
        };
    }
}
