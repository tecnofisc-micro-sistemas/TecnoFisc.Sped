// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<Armamento> ReadArmamentoAsync(CancellationToken ct)
    {
        string? extraTpArma = null;
        string? extraNSerie = null;
        string? extraNCano = null;
        string? extraDescr = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "tpArma":
                    extraTpArma = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "nSerie":
                    extraNSerie = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "nCano":
                    extraNCano = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "descr":
                    extraDescr = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new Armamento
        {
            TpArma = extraTpArma,
            NSerie = extraNSerie,
            NCano = extraNCano,
            Descr = extraDescr
        };
    }
}
