// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<ResponsavelTecnico> ReadResponsavelTecnicoAsync(CancellationToken ct)
    {
        string? extraCNPJ = null;
        string? extraXContato = null;
        string? extraEmail = null;
        string? extraFone = null;
        string? extraIdCSRT = null;
        string? extraHashCSRT = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CNPJ": extraCNPJ = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "xContato": extraXContato = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "email": extraEmail = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "fone": extraFone = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "idCSRT": extraIdCSRT = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "hashCSRT": extraHashCSRT = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new ResponsavelTecnico
        {
            CNPJ = extraCNPJ,
            XContato = extraXContato,
            Email = extraEmail,
            Fone = extraFone,
            IdCSRT = extraIdCSRT,
            HashCSRT = extraHashCSRT
        };
    }
}
