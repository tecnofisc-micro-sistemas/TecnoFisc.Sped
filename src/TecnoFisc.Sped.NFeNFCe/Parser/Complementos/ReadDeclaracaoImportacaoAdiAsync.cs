// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<DeclaracaoImportacaoAdi> ReadDeclaracaoImportacaoAdiAsync(CancellationToken ct)
    {
        string? extraNAdicao = null;
        string? extraNSeqAdic = null;
        string? extraCFabricante = null;
        decimal? extraVDescDI = null;
        string? extraNDraw = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "nAdicao":
                    extraNAdicao = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "nSeqAdic":
                    extraNSeqAdic = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "cFabricante":
                    extraCFabricante = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "vDescDI":
                    extraVDescDI = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "nDraw":
                    extraNDraw = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new DeclaracaoImportacaoAdi
        {
            NAdicao = extraNAdicao,
            NSeqAdic = extraNSeqAdic,
            CFabricante = extraCFabricante,
            VDescDI = extraVDescDI,
            NDraw = extraNDraw
        };
    }
}
