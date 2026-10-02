using System.Globalization;
// Scaffolding revisÃƒÂ¡vel a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<InfNFeAvulsa> ReadInfNFeAvulsaAsync(CancellationToken ct)
    {
        string? extraCNPJ = null;
        string? extraXOrgao = null;
        string? extraMatr = null;
        string? extraXAgente = null;
        string? extraFone = null;
        string? extraUF = null;
        string? extraNDAR = null;
        DateOnly? extraDEmi = null;
        decimal? extraVDAR = null;
        string? extraRepEmi = null;
        DateOnly? extraDPag = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CNPJ": extraCNPJ = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "xOrgao": extraXOrgao = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "matr": extraMatr = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "xAgente": extraXAgente = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "fone": extraFone = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "UF": extraUF = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "nDAR": extraNDAR = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "dEmi": extraDEmi = DateOnly.Parse(await _r.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture); return true;
                case "vDAR": extraVDAR = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "repEmi": extraRepEmi = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "dPag": extraDPag = DateOnly.Parse(await _r.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new InfNFeAvulsa
        {
            CNPJ = extraCNPJ,
            XOrgao = extraXOrgao,
            Matr = extraMatr,
            XAgente = extraXAgente,
            Fone = extraFone,
            UF = extraUF,
            NDAR = extraNDAR,
            DEmi = extraDEmi,
            VDAR = extraVDAR,
            RepEmi = extraRepEmi,
            DPag = extraDPag
        };
    }
}
