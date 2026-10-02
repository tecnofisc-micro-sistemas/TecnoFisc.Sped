// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<Intermediador> ReadIntermediadorAsync(CancellationToken ct)
    {
        string? extraCNPJ = null;
        string? extraIdCadIntTran = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CNPJ": extraCNPJ = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "idCadIntTran": extraIdCadIntTran = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new Intermediador
        {
            CNPJ = extraCNPJ,
            IdCadIntTran = extraIdCadIntTran
        };
    }
}
