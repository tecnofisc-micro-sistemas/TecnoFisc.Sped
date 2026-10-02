using System.Globalization;
// Scaffolding revisÃƒÂ¡vel a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<DeclaracaoImportacao> ReadDeclaracaoImportacaoAsync(CancellationToken ct)
    {
        string? extraNDI = null;
        DateOnly? extraDDI = null;
        string? extraXLocDesemb = null;
        string? extraUFDesemb = null;
        DateOnly? extraDDesemb = null;
        string? extraTpViaTransp = null;
        decimal? extraVAFRMM = null;
        string? extraTpIntermedio = null;
        string? extraCNPJ = null;
        string? extraCPF = null;
        string? extraUFTerceiro = null;
        string? extraCExportador = null;
        List<DeclaracaoImportacaoAdi>? extraAdi = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "nDI":
                    extraNDI = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "dDI":
                    extraDDI = DateOnly.Parse(await _r.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture);
                    return true;
                case "xLocDesemb":
                    extraXLocDesemb = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "UFDesemb":
                    extraUFDesemb = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "dDesemb":
                    extraDDesemb = DateOnly.Parse(await _r.ReadTextAsync().ConfigureAwait(false), CultureInfo.InvariantCulture);
                    return true;
                case "tpViaTransp":
                    extraTpViaTransp = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "vAFRMM":
                    extraVAFRMM = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "tpIntermedio":
                    extraTpIntermedio = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "CNPJ":
                    extraCNPJ = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "CPF":
                    extraCPF = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "UFTerceiro":
                    extraUFTerceiro = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "cExportador":
                    extraCExportador = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "adi":
                    (extraAdi ??= []).Add(await ReadDeclaracaoImportacaoAdiAsync(ct).ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new DeclaracaoImportacao
        {
            NDI = extraNDI,
            DDI = extraDDI,
            XLocDesemb = extraXLocDesemb,
            UFDesemb = extraUFDesemb,
            DDesemb = extraDDesemb,
            TpViaTransp = extraTpViaTransp,
            VAFRMM = extraVAFRMM,
            TpIntermedio = extraTpIntermedio,
            CNPJ = extraCNPJ,
            CPF = extraCPF,
            UFTerceiro = extraUFTerceiro,
            CExportador = extraCExportador,
            Adi = (IReadOnlyList<DeclaracaoImportacaoAdi>?)extraAdi ?? []
        };
    }
}
