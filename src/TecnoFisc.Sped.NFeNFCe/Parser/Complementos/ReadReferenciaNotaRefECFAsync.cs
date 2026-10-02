// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<ReferenciaNotaRefECF> ReadReferenciaNotaRefECFAsync(CancellationToken ct)
    {
        string? extraMod = null;
        string? extraNECF = null;
        string? extraNCOO = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "mod":
                    extraMod = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "nECF":
                    extraNECF = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "nCOO":
                    extraNCOO = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new ReferenciaNotaRefECF
        {
            Mod = extraMod,
            NECF = extraNECF,
            NCOO = extraNCOO
        };
    }
}
