// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<InfNFeInfPAA> ReadInfNFeInfPAAAsync(CancellationToken ct)
    {
        string? extraCNPJPAA = null;
        InfNFeInfPAAPAASignature? extraPAASignature = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CNPJPAA":
                    extraCNPJPAA = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "PAASignature":
                    extraPAASignature = await ReadInfNFeInfPAAPAASignatureAsync(ct).ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new InfNFeInfPAA
        {
            CNPJPAA = extraCNPJPAA,
            PAASignature = extraPAASignature
        };
    }
}
