// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGCredPresOperGIBSCredPres> ReadTributacaoIBSCBSGCredPresOperGIBSCredPresAsync(CancellationToken ct)
    {
        decimal? extraPCredPres = null;
        decimal? extraVCredPres = null;
        decimal? extraVCredPresCondSus = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "pCredPres":
                    extraPCredPres = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vCredPres":
                    extraVCredPres = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vCredPresCondSus":
                    extraVCredPresCondSus = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGCredPresOperGIBSCredPres
        {
            PCredPres = extraPCredPres,
            VCredPres = extraVCredPres,
            VCredPresCondSus = extraVCredPresCondSus
        };
    }
}
