// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TransporteReboque> ReadTransporteReboqueAsync(CancellationToken ct)
    {
        string? extraPlaca = null;
        string? extraUF = null;
        string? extraRNTC = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "placa":
                    extraPlaca = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "UF":
                    extraUF = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "RNTC":
                    extraRNTC = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TransporteReboque
        {
            Placa = extraPlaca,
            UF = extraUF,
            RNTC = extraRNTC
        };
    }
}
