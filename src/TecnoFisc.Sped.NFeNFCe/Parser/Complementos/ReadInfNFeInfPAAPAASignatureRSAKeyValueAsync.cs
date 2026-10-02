// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<InfNFeInfPAAPAASignatureRSAKeyValue> ReadInfNFeInfPAAPAASignatureRSAKeyValueAsync(CancellationToken ct)
    {
        string? extraModulus = null;
        string? extraExponent = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "Modulus": extraModulus = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "Exponent": extraExponent = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new InfNFeInfPAAPAASignatureRSAKeyValue
        {
            Modulus = extraModulus,
            Exponent = extraExponent
        };
    }
}
