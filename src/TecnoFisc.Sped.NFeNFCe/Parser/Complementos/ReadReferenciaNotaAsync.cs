// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<ReferenciaNota> ReadReferenciaNotaAsync(CancellationToken ct)
    {
        string? extraRefNFe = null;
        string? extraRefNFeSig = null;
        ReferenciaNotaRefNF? extraRefNF = null;
        ReferenciaNotaRefNFP? extraRefNFP = null;
        string? extraRefCTe = null;
        ReferenciaNotaRefECF? extraRefECF = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "refNFe": extraRefNFe = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "refNFeSig": extraRefNFeSig = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "refNF": extraRefNF = await ReadReferenciaNotaRefNFAsync(ct).ConfigureAwait(false); return true;
                case "refNFP": extraRefNFP = await ReadReferenciaNotaRefNFPAsync(ct).ConfigureAwait(false); return true;
                case "refCTe": extraRefCTe = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "refECF": extraRefECF = await ReadReferenciaNotaRefECFAsync(ct).ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new ReferenciaNota
        {
            RefNFe = extraRefNFe,
            RefNFeSig = extraRefNFeSig,
            RefNF = extraRefNF,
            RefNFP = extraRefNFP,
            RefCTe = extraRefCTe,
            RefECF = extraRefECF
        };
    }
}
