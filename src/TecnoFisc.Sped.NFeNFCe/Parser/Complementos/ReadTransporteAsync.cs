// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<Transporte> ReadTransporteAsync(CancellationToken ct)
    {
        string? extraModFrete = null;
        TransporteTransporta? extraTransporta = null;
        TransporteRetTransp? extraRetTransp = null;
        TransporteVeicTransp? extraVeicTransp = null;
        List<TransporteReboque>? extraReboque = null;
        string? extraVagao = null;
        string? extraBalsa = null;
        List<TransporteVol>? extraVol = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "modFrete":
                    extraModFrete = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "transporta":
                    extraTransporta = await ReadTransporteTransportaAsync(ct).ConfigureAwait(false);
                    return true;
                case "retTransp":
                    extraRetTransp = await ReadTransporteRetTranspAsync(ct).ConfigureAwait(false);
                    return true;
                case "veicTransp":
                    extraVeicTransp = await ReadTransporteVeicTranspAsync(ct).ConfigureAwait(false);
                    return true;
                case "reboque":
                    (extraReboque ??= []).Add(await ReadTransporteReboqueAsync(ct).ConfigureAwait(false));
                    return true;
                case "vagao":
                    extraVagao = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "balsa":
                    extraBalsa = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "vol":
                    (extraVol ??= []).Add(await ReadTransporteVolAsync(ct).ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new Transporte
        {
            ModFrete = extraModFrete,
            Transporta = extraTransporta,
            RetTransp = extraRetTransp,
            VeicTransp = extraVeicTransp,
            Reboque = (IReadOnlyList<TransporteReboque>?)extraReboque ?? [],
            Vagao = extraVagao,
            Balsa = extraBalsa,
            Vol = (IReadOnlyList<TransporteVol>?)extraVol ?? []
        };
    }
}
