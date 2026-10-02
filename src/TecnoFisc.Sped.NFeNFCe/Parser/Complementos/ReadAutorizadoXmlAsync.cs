// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<AutorizadoXml> ReadAutorizadoXmlAsync(CancellationToken ct)
    {
        string? extraCNPJ = null;
        string? extraCPF = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CNPJ":
                    extraCNPJ = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "CPF":
                    extraCPF = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new AutorizadoXml
        {
            CNPJ = extraCNPJ,
            CPF = extraCPF
        };
    }
}
