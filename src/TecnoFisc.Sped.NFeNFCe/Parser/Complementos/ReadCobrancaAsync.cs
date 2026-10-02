// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<Cobranca> ReadCobrancaAsync(CancellationToken ct)
    {
        CobrancaFat? extraFat = null;
        List<CobrancaDup>? extraDup = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "fat": extraFat = await ReadCobrancaFatAsync(ct).ConfigureAwait(false); return true;
                case "dup": (extraDup ??= []).Add(await ReadCobrancaDupAsync(ct).ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new Cobranca
        {
            Fat = extraFat,
            Dup = (IReadOnlyList<CobrancaDup>?)extraDup ?? []
        };
    }
}
