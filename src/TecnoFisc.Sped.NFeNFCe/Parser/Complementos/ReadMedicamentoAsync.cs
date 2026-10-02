// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<Medicamento> ReadMedicamentoAsync(CancellationToken ct)
    {
        string? extraCProdANVISA = null;
        string? extraXMotivoIsencao = null;
        decimal? extraVPMC = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "cProdANVISA": extraCProdANVISA = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "xMotivoIsencao": extraXMotivoIsencao = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "vPMC": extraVPMC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new Medicamento
        {
            CProdANVISA = extraCProdANVISA,
            XMotivoIsencao = extraXMotivoIsencao,
            VPMC = extraVPMC
        };
    }
}
