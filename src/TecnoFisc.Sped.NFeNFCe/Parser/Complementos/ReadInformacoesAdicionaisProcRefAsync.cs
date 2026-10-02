// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<InformacoesAdicionaisProcRef> ReadInformacoesAdicionaisProcRefAsync(CancellationToken ct)
    {
        string? extraNProc = null;
        string? extraIndProc = null;
        string? extraTpAto = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "nProc": extraNProc = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "indProc": extraIndProc = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "tpAto": extraTpAto = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new InformacoesAdicionaisProcRef
        {
            NProc = extraNProc,
            IndProc = extraIndProc,
            TpAto = extraTpAto
        };
    }
}
