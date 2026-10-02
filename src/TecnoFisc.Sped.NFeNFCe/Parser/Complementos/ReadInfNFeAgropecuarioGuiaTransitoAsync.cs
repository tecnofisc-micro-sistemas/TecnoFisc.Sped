// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<InfNFeAgropecuarioGuiaTransito> ReadInfNFeAgropecuarioGuiaTransitoAsync(CancellationToken ct)
    {
        string? extraTpGuia = null;
        string? extraUFGuia = null;
        string? extraSerieGuia = null;
        string? extraNGuia = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "tpGuia":
                    extraTpGuia = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "UFGuia":
                    extraUFGuia = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "serieGuia":
                    extraSerieGuia = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "nGuia":
                    extraNGuia = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new InfNFeAgropecuarioGuiaTransito
        {
            TpGuia = extraTpGuia,
            UFGuia = extraUFGuia,
            SerieGuia = extraSerieGuia,
            NGuia = extraNGuia
        };
    }
}
