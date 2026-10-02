// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<LocalEntrega> ReadLocalEntregaAsync(CancellationToken ct)
    {
        string? extraCNPJ = null;
        string? extraCPF = null;
        string? extraXNome = null;
        string? extraXLgr = null;
        string? extraNro = null;
        string? extraXCpl = null;
        string? extraXBairro = null;
        string? extraCMun = null;
        string? extraXMun = null;
        string? extraUF = null;
        string? extraCEP = null;
        string? extraCPais = null;
        string? extraXPais = null;
        string? extraFone = null;
        string? extraEmail = null;
        string? extraIE = null;
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
                case "xNome":
                    extraXNome = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "xLgr":
                    extraXLgr = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "nro":
                    extraNro = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "xCpl":
                    extraXCpl = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "xBairro":
                    extraXBairro = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "cMun":
                    extraCMun = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "xMun":
                    extraXMun = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "UF":
                    extraUF = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "CEP":
                    extraCEP = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "cPais":
                    extraCPais = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "xPais":
                    extraXPais = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "fone":
                    extraFone = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "email":
                    extraEmail = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "IE":
                    extraIE = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new LocalEntrega
        {
            CNPJ = extraCNPJ,
            CPF = extraCPF,
            XNome = extraXNome,
            XLgr = extraXLgr,
            Nro = extraNro,
            XCpl = extraXCpl,
            XBairro = extraXBairro,
            CMun = extraCMun,
            XMun = extraXMun,
            UF = extraUF,
            CEP = extraCEP,
            CPais = extraCPais,
            XPais = extraXPais,
            Fone = extraFone,
            Email = extraEmail,
            IE = extraIE
        };
    }
}
