// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<InfNFeAgropecuarioDefensivo> ReadInfNFeAgropecuarioDefensivoAsync(CancellationToken ct)
    {
        string? extraNReceituario = null;
        string? extraCPFRespTec = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "nReceituario":
                    extraNReceituario = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "CPFRespTec":
                    extraCPFRespTec = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new InfNFeAgropecuarioDefensivo
        {
            NReceituario = extraNReceituario,
            CPFRespTec = extraCPFRespTec
        };
    }
}
