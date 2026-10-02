// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<Combustivel> ReadCombustivelAsync(CancellationToken ct)
    {
        string? extraCProdANP = null;
        string? extraDescANP = null;
        decimal? extraPGLP = null;
        decimal? extraPGNn = null;
        decimal? extraPGNi = null;
        decimal? extraVPart = null;
        string? extraCODIF = null;
        decimal? extraQTemp = null;
        string? extraUFCons = null;
        CombustivelCIDE? extraCIDE = null;
        CombustivelEncerrante? extraEncerrante = null;
        decimal? extraPBio = null;
        List<CombustivelOrigComb>? extraOrigComb = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "cProdANP": extraCProdANP = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "descANP": extraDescANP = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "pGLP": extraPGLP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pGNn": extraPGNn = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pGNi": extraPGNi = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vPart": extraVPart = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CODIF": extraCODIF = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "qTemp": extraQTemp = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "UFCons": extraUFCons = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "CIDE": extraCIDE = await ReadCombustivelCIDEAsync(ct).ConfigureAwait(false); return true;
                case "encerrante": extraEncerrante = await ReadCombustivelEncerranteAsync(ct).ConfigureAwait(false); return true;
                case "pBio": extraPBio = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "origComb": (extraOrigComb ??= []).Add(await ReadCombustivelOrigCombAsync(ct).ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new Combustivel
        {
            CProdANP = extraCProdANP,
            DescANP = extraDescANP,
            PGLP = extraPGLP,
            PGNn = extraPGNn,
            PGNi = extraPGNi,
            VPart = extraVPart,
            CODIF = extraCODIF,
            QTemp = extraQTemp,
            UFCons = extraUFCons,
            CIDE = extraCIDE,
            Encerrante = extraEncerrante,
            PBio = extraPBio,
            OrigComb = (IReadOnlyList<CombustivelOrigComb>?)extraOrigComb ?? []
        };
    }
}
