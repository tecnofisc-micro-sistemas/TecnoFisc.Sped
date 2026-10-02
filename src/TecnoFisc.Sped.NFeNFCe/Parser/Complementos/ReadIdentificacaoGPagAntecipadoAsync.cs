// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<IdentificacaoGPagAntecipado> ReadIdentificacaoGPagAntecipadoAsync(CancellationToken ct)
    {
        List<string>? extraRefNFe = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "refNFe":
                    (extraRefNFe ??= []).Add(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new IdentificacaoGPagAntecipado
        {
            RefNFe = (IReadOnlyList<string>?)extraRefNFe ?? []
        };
    }
}
