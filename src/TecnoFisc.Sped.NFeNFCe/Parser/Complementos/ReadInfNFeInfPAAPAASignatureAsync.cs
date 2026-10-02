// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<InfNFeInfPAAPAASignature> ReadInfNFeInfPAAPAASignatureAsync(CancellationToken ct)
    {
        string? extraSignatureValue = null;
        InfNFeInfPAAPAASignatureRSAKeyValue? extraRSAKeyValue = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "SignatureValue": extraSignatureValue = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "RSAKeyValue": extraRSAKeyValue = await ReadInfNFeInfPAAPAASignatureRSAKeyValueAsync(ct).ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new InfNFeInfPAAPAASignature
        {
            SignatureValue = extraSignatureValue,
            RSAKeyValue = extraRSAKeyValue
        };
    }
}
