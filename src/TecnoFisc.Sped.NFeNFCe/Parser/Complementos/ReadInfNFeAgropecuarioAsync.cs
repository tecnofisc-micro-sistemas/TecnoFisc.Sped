// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<InfNFeAgropecuario> ReadInfNFeAgropecuarioAsync(CancellationToken ct)
    {
        List<InfNFeAgropecuarioDefensivo>? extraDefensivo = null;
        InfNFeAgropecuarioGuiaTransito? extraGuiaTransito = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "defensivo":
                    (extraDefensivo ??= []).Add(await ReadInfNFeAgropecuarioDefensivoAsync(ct).ConfigureAwait(false));
                    return true;
                case "guiaTransito":
                    extraGuiaTransito = await ReadInfNFeAgropecuarioGuiaTransitoAsync(ct).ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new InfNFeAgropecuario
        {
            Defensivo = (IReadOnlyList<InfNFeAgropecuarioDefensivo>?)extraDefensivo ?? [],
            GuiaTransito = extraGuiaTransito
        };
    }
}
