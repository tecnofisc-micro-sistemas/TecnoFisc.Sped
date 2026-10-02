// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<ReferenciaNotaRefNF> ReadReferenciaNotaRefNFAsync(CancellationToken ct)
    {
        string? extraCUF = null;
        string? extraAAMM = null;
        string? extraCNPJ = null;
        string? extraMod = null;
        string? extraSerie = null;
        string? extraNNF = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "cUF": extraCUF = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "AAMM": extraAAMM = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CNPJ": extraCNPJ = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "mod": extraMod = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "serie": extraSerie = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "nNF": extraNNF = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new ReferenciaNotaRefNF
        {
            CUF = extraCUF,
            AAMM = extraAAMM,
            CNPJ = extraCNPJ,
            Mod = extraMod,
            Serie = extraSerie,
            NNF = extraNNF
        };
    }
}
