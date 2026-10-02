// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<InformacaoSuplementar> ReadInformacaoSuplementarAsync(CancellationToken ct)
    {
        string? extraQrCode = null;
        string? extraUrlChave = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "qrCode": extraQrCode = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "urlChave": extraUrlChave = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new InformacaoSuplementar
        {
            QrCode = extraQrCode,
            UrlChave = extraUrlChave
        };
    }
}
