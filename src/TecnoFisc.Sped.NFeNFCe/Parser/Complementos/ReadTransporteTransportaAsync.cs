// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TransporteTransporta> ReadTransporteTransportaAsync(CancellationToken ct)
    {
        string? extraCNPJ = null;
        string? extraCPF = null;
        string? extraXNome = null;
        string? extraIE = null;
        string? extraXEnder = null;
        string? extraXMun = null;
        string? extraUF = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CNPJ": extraCNPJ = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CPF": extraCPF = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "xNome": extraXNome = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "IE": extraIE = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "xEnder": extraXEnder = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "xMun": extraXMun = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "UF": extraUF = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TransporteTransporta
        {
            CNPJ = extraCNPJ,
            CPF = extraCPF,
            XNome = extraXNome,
            IE = extraIE,
            XEnder = extraXEnder,
            XMun = extraXMun,
            UF = extraUF
        };
    }
}
