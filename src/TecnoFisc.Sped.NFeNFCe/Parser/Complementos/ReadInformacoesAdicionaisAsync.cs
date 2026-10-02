// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<InformacoesAdicionais> ReadInformacoesAdicionaisAsync(CancellationToken ct)
    {
        string? extraInfAdFisco = null;
        string? extraInfCpl = null;
        List<InformacoesAdicionaisObsCont>? extraObsCont = null;
        List<InformacoesAdicionaisObsFisco>? extraObsFisco = null;
        List<InformacoesAdicionaisProcRef>? extraProcRef = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "infAdFisco": extraInfAdFisco = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "infCpl": extraInfCpl = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "obsCont": (extraObsCont ??= []).Add(await ReadInformacoesAdicionaisObsContAsync(ct).ConfigureAwait(false)); return true;
                case "obsFisco": (extraObsFisco ??= []).Add(await ReadInformacoesAdicionaisObsFiscoAsync(ct).ConfigureAwait(false)); return true;
                case "procRef": (extraProcRef ??= []).Add(await ReadInformacoesAdicionaisProcRefAsync(ct).ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new InformacoesAdicionais
        {
            InfAdFisco = extraInfAdFisco,
            InfCpl = extraInfCpl,
            ObsCont = (IReadOnlyList<InformacoesAdicionaisObsCont>?)extraObsCont ?? [],
            ObsFisco = (IReadOnlyList<InformacoesAdicionaisObsFisco>?)extraObsFisco ?? [],
            ProcRef = (IReadOnlyList<InformacoesAdicionaisProcRef>?)extraProcRef ?? []
        };
    }
}
