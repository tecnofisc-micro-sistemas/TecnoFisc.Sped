using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<Icms?> ReadIcmsAsync(CancellationToken ct)
    {
        Icms? icms = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "ICMS00": icms = await ReadIcms00Async(ct).ConfigureAwait(false); return true;
                case "ICMS10": icms = await ReadIcms10Async(ct).ConfigureAwait(false); return true;
                case "ICMS20": icms = await ReadIcms20Async(ct).ConfigureAwait(false); return true;
                case "ICMS30": icms = await ReadIcms30Async(ct).ConfigureAwait(false); return true;
                case "ICMS40": icms = await ReadIcms40Async(ct).ConfigureAwait(false); return true;
                case "ICMS51": icms = await ReadIcms51Async(ct).ConfigureAwait(false); return true;
                case "ICMS60": icms = await ReadIcms60Async(ct).ConfigureAwait(false); return true;
                case "ICMS70": icms = await ReadIcms70Async(ct).ConfigureAwait(false); return true;
                case "ICMS90": icms = await ReadIcms90Async(ct).ConfigureAwait(false); return true;
                case "ICMSPart": icms = await ReadIcmsPartAsync(ct).ConfigureAwait(false); return true;
                case "ICMSST": icms = await ReadIcmsSTAsync(ct).ConfigureAwait(false); return true;
                case "ICMSSN101": icms = await ReadIcmsSN101Async(ct).ConfigureAwait(false); return true;
                case "ICMSSN102": icms = await ReadIcmsSN102Async(ct).ConfigureAwait(false); return true;
                case "ICMSSN201": icms = await ReadIcmsSN201Async(ct).ConfigureAwait(false); return true;
                case "ICMSSN202": icms = await ReadIcmsSN202Async(ct).ConfigureAwait(false); return true;
                case "ICMSSN500": icms = await ReadIcmsSN500Async(ct).ConfigureAwait(false); return true;
                case "ICMSSN900": icms = await ReadIcmsSN900Async(ct).ConfigureAwait(false); return true;
                // Variantes combustível (ICMS02/15/53/61) entram em slices posteriores.
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return icms;
    }

    private async Task<Icms00> ReadIcms00Async(CancellationToken ct)
    {
        OrigemMercadoria orig = default; string cstTributacao = string.Empty;
        int modBC = 0; decimal vBC = 0, pICMS = 0, vICMS = 0;
        decimal? pFCP = null, vFCP = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CST": cstTributacao = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "modBC": modBC = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBC": vBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMS": pICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMS": vICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCP": pFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCP": vFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        Cst cst = CombinarCst(orig, cstTributacao);

        return new Icms00
        {
            Orig = orig, CST = cst, ModBC = modBC, VBC = vBC, PICMS = pICMS, VICMS = vICMS,
            PFCP = pFCP, VFCP = vFCP,
        };
    }

    private async Task<Icms10> ReadIcms10Async(CancellationToken ct)
    {
        OrigemMercadoria orig = default; string cstTributacao = string.Empty;
        int modBC = 0, modBCST = 0;
        decimal vBC = 0, pICMS = 0, vICMS = 0, vBCST = 0, pICMSST = 0, vICMSST = 0;
        decimal? vBCFCP = null, pFCP = null, vFCP = null;
        decimal? pMVAST = null, pRedBCST = null;
        decimal? vBCFCPST = null, pFCPST = null, vFCPST = null;
        decimal? vICMSSTDeson = null;
        int? motDesICMSST = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CST": cstTributacao = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "modBC": modBC = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBC": vBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMS": pICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMS": vICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCFCP": vBCFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCP": pFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCP": vFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "modBCST": modBCST = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pMVAST": pMVAST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBCST": pRedBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCST": vBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMSST": pICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSST": vICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCFCPST": vBCFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCPST": pFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPST": vFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSSTDeson": vICMSSTDeson = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "motDesICMSST": motDesICMSST = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        Cst cst = CombinarCst(orig, cstTributacao);

        return new Icms10
        {
            Orig = orig, CST = cst, ModBC = modBC, VBC = vBC, PICMS = pICMS, VICMS = vICMS,
            VBCFCP = vBCFCP, PFCP = pFCP, VFCP = vFCP,
            ModBCST = modBCST, PMVAST = pMVAST, PRedBCST = pRedBCST,
            VBCST = vBCST, PICMSST = pICMSST, VICMSST = vICMSST,
            VBCFCPST = vBCFCPST, PFCPST = pFCPST, VFCPST = vFCPST,
            VICMSSTDeson = vICMSSTDeson, MotDesICMSST = motDesICMSST,
        };
    }

    private async Task<Icms20> ReadIcms20Async(CancellationToken ct)
    {
        OrigemMercadoria orig = default; string cstTributacao = string.Empty;
        int modBC = 0;
        decimal pRedBC = 0, vBC = 0, pICMS = 0, vICMS = 0;
        decimal? vBCFCP = null, pFCP = null, vFCP = null;
        decimal? vICMSDeson = null;
        int? motDesICMS = null, indDeduzDeson = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CST": cstTributacao = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "modBC": modBC = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBC": pRedBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBC": vBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMS": pICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMS": vICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCFCP": vBCFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCP": pFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCP": vFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSDeson": vICMSDeson = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "motDesICMS": motDesICMS = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indDeduzDeson": indDeduzDeson = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        Cst cst = CombinarCst(orig, cstTributacao);

        return new Icms20
        {
            Orig = orig, CST = cst, ModBC = modBC, PRedBC = pRedBC, VBC = vBC, PICMS = pICMS, VICMS = vICMS,
            VBCFCP = vBCFCP, PFCP = pFCP, VFCP = vFCP,
            VICMSDeson = vICMSDeson, MotDesICMS = motDesICMS, IndDeduzDeson = indDeduzDeson,
        };
    }

    private async Task<Icms30> ReadIcms30Async(CancellationToken ct)
    {
        OrigemMercadoria orig = default; string cstTributacao = string.Empty;
        int modBCST = 0;
        decimal vBCST = 0, pICMSST = 0, vICMSST = 0;
        decimal? pMVAST = null, pRedBCST = null;
        decimal? vBCFCPST = null, pFCPST = null, vFCPST = null;
        decimal? vICMSDeson = null;
        int? motDesICMS = null, indDeduzDeson = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CST": cstTributacao = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "modBCST": modBCST = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pMVAST": pMVAST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBCST": pRedBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCST": vBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMSST": pICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSST": vICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCFCPST": vBCFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCPST": pFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPST": vFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSDeson": vICMSDeson = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "motDesICMS": motDesICMS = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indDeduzDeson": indDeduzDeson = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        Cst cst = CombinarCst(orig, cstTributacao);

        return new Icms30
        {
            Orig = orig, CST = cst, ModBCST = modBCST, PMVAST = pMVAST, PRedBCST = pRedBCST,
            VBCST = vBCST, PICMSST = pICMSST, VICMSST = vICMSST,
            VBCFCPST = vBCFCPST, PFCPST = pFCPST, VFCPST = vFCPST,
            VICMSDeson = vICMSDeson, MotDesICMS = motDesICMS, IndDeduzDeson = indDeduzDeson,
        };
    }

    private async Task<Icms40> ReadIcms40Async(CancellationToken ct)
    {
        OrigemMercadoria orig = default; string cstTributacao = string.Empty;
        decimal? vICMSDeson = null;
        int? motDesICMS = null, indDeduzDeson = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CST": cstTributacao = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "vICMSDeson": vICMSDeson = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "motDesICMS": motDesICMS = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indDeduzDeson": indDeduzDeson = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        Cst cst = CombinarCst(orig, cstTributacao);

        return new Icms40
        {
            Orig = orig, CST = cst,
            VICMSDeson = vICMSDeson, MotDesICMS = motDesICMS, IndDeduzDeson = indDeduzDeson,
        };
    }

    private async Task<Icms51> ReadIcms51Async(CancellationToken ct)
    {
        OrigemMercadoria orig = default; string cstTributacao = string.Empty;
        int? modBC = null;
        decimal? pRedBC = null;
        string? cBenefRBC = null;
        decimal? vBC = null, pICMS = null, vICMSOp = null, pDif = null, vICMSDif = null, vICMS = null;
        decimal? vBCFCP = null, pFCP = null, vFCP = null;
        decimal? pFCPDif = null, vFCPDif = null, vFCPEfet = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CST": cstTributacao = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "modBC": modBC = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBC": pRedBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "cBenefRBC": cBenefRBC = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "vBC": vBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMS": pICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSOp": vICMSOp = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pDif": pDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSDif": vICMSDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMS": vICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCFCP": vBCFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCP": pFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCP": vFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCPDif": pFCPDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPDif": vFCPDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPEfet": vFCPEfet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        Cst cst = CombinarCst(orig, cstTributacao);

        return new Icms51
        {
            Orig = orig, CST = cst,
            ModBC = modBC, PRedBC = pRedBC, CBenefRBC = cBenefRBC,
            VBC = vBC, PICMS = pICMS, VICMSOp = vICMSOp, PDif = pDif, VICMSDif = vICMSDif, VICMS = vICMS,
            VBCFCP = vBCFCP, PFCP = pFCP, VFCP = vFCP,
            PFCPDif = pFCPDif, VFCPDif = vFCPDif, VFCPEfet = vFCPEfet,
        };
    }

    private async Task<Icms60> ReadIcms60Async(CancellationToken ct)
    {
        OrigemMercadoria orig = default; string cstTributacao = string.Empty;
        decimal? vBCSTRet = null, pST = null, vICMSSubstituto = null, vICMSSTRet = null,
            pRedBCEfet = null, vBCEfet = null, pICMSEfet = null, vICMSEfet = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CST": cstTributacao = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "vBCSTRet": vBCSTRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pST": pST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSSubstituto": vICMSSubstituto = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSSTRet": vICMSSTRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBCEfet": pRedBCEfet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCEfet": vBCEfet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMSEfet": pICMSEfet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSEfet": vICMSEfet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        Cst cst = CombinarCst(orig, cstTributacao);

        return new Icms60
        {
            Orig = orig, CST = cst, VBCSTRet = vBCSTRet, PST = pST,
            VICMSSubstituto = vICMSSubstituto, VICMSSTRet = vICMSSTRet,
            PRedBCEfet = pRedBCEfet, VBCEfet = vBCEfet, PICMSEfet = pICMSEfet, VICMSEfet = vICMSEfet,
        };
    }

    private async Task<Icms70> ReadIcms70Async(CancellationToken ct)
    {
        OrigemMercadoria orig = default; string cstTributacao = string.Empty;
        int modBC = 0, modBCST = 0;
        decimal pRedBC = 0, vBC = 0, pICMS = 0, vICMS = 0;
        decimal? vBCFCP = null, pFCP = null, vFCP = null;
        decimal? pMVAST = null, pRedBCST = null;
        decimal vBCST = 0, pICMSST = 0, vICMSST = 0;
        decimal? vBCFCPST = null, pFCPST = null, vFCPST = null;
        decimal? vICMSDeson = null;
        int? motDesICMS = null, indDeduzDeson = null;
        decimal? vICMSSTDeson = null;
        int? motDesICMSST = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CST": cstTributacao = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "modBC": modBC = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBC": pRedBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBC": vBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMS": pICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMS": vICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCFCP": vBCFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCP": pFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCP": vFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "modBCST": modBCST = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pMVAST": pMVAST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBCST": pRedBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCST": vBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMSST": pICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSST": vICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCFCPST": vBCFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCPST": pFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPST": vFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSDeson": vICMSDeson = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "motDesICMS": motDesICMS = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indDeduzDeson": indDeduzDeson = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSSTDeson": vICMSSTDeson = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "motDesICMSST": motDesICMSST = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        Cst cst = CombinarCst(orig, cstTributacao);

        return new Icms70
        {
            Orig = orig, CST = cst,
            ModBC = modBC, PRedBC = pRedBC, VBC = vBC, PICMS = pICMS, VICMS = vICMS,
            VBCFCP = vBCFCP, PFCP = pFCP, VFCP = vFCP,
            ModBCST = modBCST, PMVAST = pMVAST, PRedBCST = pRedBCST,
            VBCST = vBCST, PICMSST = pICMSST, VICMSST = vICMSST,
            VBCFCPST = vBCFCPST, PFCPST = pFCPST, VFCPST = vFCPST,
            VICMSDeson = vICMSDeson, MotDesICMS = motDesICMS, IndDeduzDeson = indDeduzDeson,
            VICMSSTDeson = vICMSSTDeson, MotDesICMSST = motDesICMSST,
        };
    }

    private async Task<Icms90> ReadIcms90Async(CancellationToken ct)
    {
        OrigemMercadoria orig = default; string cstTributacao = string.Empty;
        int? modBC = null, modBCST = null;
        decimal? vBC = null, pRedBC = null;
        string? cBenefRBC = null;
        decimal? pICMS = null, vICMSOp = null, pDif = null, vICMSDif = null, vICMS = null;
        decimal? vBCFCP = null, pFCP = null, vFCP = null;
        decimal? pFCPDif = null, vFCPDif = null, vFCPEfet = null;
        decimal? pMVAST = null, pRedBCST = null;
        decimal? vBCST = null, pICMSST = null, vICMSST = null;
        decimal? vBCFCPST = null, pFCPST = null, vFCPST = null;
        decimal? vICMSDeson = null;
        int? motDesICMS = null, indDeduzDeson = null;
        decimal? vICMSSTDeson = null;
        int? motDesICMSST = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CST": cstTributacao = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "modBC": modBC = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBC": vBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBC": pRedBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "cBenefRBC": cBenefRBC = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "pICMS": pICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSOp": vICMSOp = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pDif": pDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSDif": vICMSDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMS": vICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCFCP": vBCFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCP": pFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCP": vFCP = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCPDif": pFCPDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPDif": vFCPDif = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPEfet": vFCPEfet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "modBCST": modBCST = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pMVAST": pMVAST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBCST": pRedBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCST": vBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMSST": pICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSST": vICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCFCPST": vBCFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCPST": pFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPST": vFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSDeson": vICMSDeson = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "motDesICMS": motDesICMS = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indDeduzDeson": indDeduzDeson = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSSTDeson": vICMSSTDeson = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "motDesICMSST": motDesICMSST = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        Cst cst = CombinarCst(orig, cstTributacao);

        return new Icms90
        {
            Orig = orig, CST = cst,
            ModBC = modBC, VBC = vBC, PRedBC = pRedBC, CBenefRBC = cBenefRBC,
            PICMS = pICMS, VICMSOp = vICMSOp, PDif = pDif, VICMSDif = vICMSDif, VICMS = vICMS,
            VBCFCP = vBCFCP, PFCP = pFCP, VFCP = vFCP,
            PFCPDif = pFCPDif, VFCPDif = vFCPDif, VFCPEfet = vFCPEfet,
            ModBCST = modBCST, PMVAST = pMVAST, PRedBCST = pRedBCST,
            VBCST = vBCST, PICMSST = pICMSST, VICMSST = vICMSST,
            VBCFCPST = vBCFCPST, PFCPST = pFCPST, VFCPST = vFCPST,
            VICMSDeson = vICMSDeson, MotDesICMS = motDesICMS, IndDeduzDeson = indDeduzDeson,
            VICMSSTDeson = vICMSSTDeson, MotDesICMSST = motDesICMSST,
        };
    }

    private async Task<IcmsPart> ReadIcmsPartAsync(CancellationToken ct)
    {
        OrigemMercadoria orig = default; string cstTributacao = string.Empty;
        int modBC = 0, modBCST = 0;
        decimal vBC = 0, pICMS = 0, vICMS = 0;
        decimal? pRedBC = null;
        decimal? pMVAST = null, pRedBCST = null;
        decimal vBCST = 0, pICMSST = 0, vICMSST = 0;
        decimal? vBCFCPST = null, pFCPST = null, vFCPST = null;
        decimal pBCOp = 0;
        string ufST = string.Empty;
        decimal? vICMSDeson = null;
        int? motDesICMS = null, indDeduzDeson = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CST": cstTributacao = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "modBC": modBC = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBC": vBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBC": pRedBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMS": pICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMS": vICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "modBCST": modBCST = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pMVAST": pMVAST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBCST": pRedBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCST": vBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMSST": pICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSST": vICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCFCPST": vBCFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCPST": pFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPST": vFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pBCOp": pBCOp = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "UFST": ufST = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "vICMSDeson": vICMSDeson = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "motDesICMS": motDesICMS = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "indDeduzDeson": indDeduzDeson = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        Cst cst = CombinarCst(orig, cstTributacao);

        return new IcmsPart
        {
            Orig = orig, CST = cst,
            ModBC = modBC, VBC = vBC, PRedBC = pRedBC, PICMS = pICMS, VICMS = vICMS,
            ModBCST = modBCST, PMVAST = pMVAST, PRedBCST = pRedBCST,
            VBCST = vBCST, PICMSST = pICMSST, VICMSST = vICMSST,
            VBCFCPST = vBCFCPST, PFCPST = pFCPST, VFCPST = vFCPST,
            PBCOp = pBCOp, UFST = ufST,
            VICMSDeson = vICMSDeson, MotDesICMS = motDesICMS, IndDeduzDeson = indDeduzDeson,
        };
    }

    // -------------------------------------------------------------------------
    // CSOSN — Simples Nacional
    // -------------------------------------------------------------------------

    private async Task<IcmsSN101> ReadIcmsSN101Async(CancellationToken ct)
    {
        OrigemMercadoria orig = default;
        string csosnTexto = string.Empty;
        decimal pCredSN = 0, vCredICMSSN = 0;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CSOSN": csosnTexto = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "pCredSN": pCredSN = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vCredICMSSN": vCredICMSSN = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new IcmsSN101
        {
            Orig = orig,
            CSOSN = Csosn.Create(csosnTexto),
            PCredSN = pCredSN,
            VCredICMSSN = vCredICMSSN,
        };
    }

    private async Task<IcmsSN102> ReadIcmsSN102Async(CancellationToken ct)
    {
        // orig é opcional (minOccurs="0") nesta variante; ausente → default Nacional(0).
        OrigemMercadoria orig = default;
        string csosnTexto = string.Empty;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CSOSN": csosnTexto = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new IcmsSN102
        {
            Orig = orig,
            CSOSN = Csosn.Create(csosnTexto),
        };
    }

    private async Task<IcmsSN201> ReadIcmsSN201Async(CancellationToken ct)
    {
        OrigemMercadoria orig = default;
        string csosnTexto = string.Empty;
        int modBCST = 0;
        decimal? pMVAST = null, pRedBCST = null;
        decimal vBCST = 0, pICMSST = 0, vICMSST = 0;
        decimal? vBCFCPST = null, pFCPST = null, vFCPST = null;
        decimal pCredSN = 0, vCredICMSSN = 0;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CSOSN": csosnTexto = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "modBCST": modBCST = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pMVAST": pMVAST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBCST": pRedBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCST": vBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMSST": pICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSST": vICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCFCPST": vBCFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCPST": pFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPST": vFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pCredSN": pCredSN = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vCredICMSSN": vCredICMSSN = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new IcmsSN201
        {
            Orig = orig,
            CSOSN = Csosn.Create(csosnTexto),
            ModBCST = modBCST,
            PMVAST = pMVAST, PRedBCST = pRedBCST,
            VBCST = vBCST, PICMSST = pICMSST, VICMSST = vICMSST,
            VBCFCPST = vBCFCPST, PFCPST = pFCPST, VFCPST = vFCPST,
            PCredSN = pCredSN, VCredICMSSN = vCredICMSSN,
        };
    }

    private async Task<IcmsSN202> ReadIcmsSN202Async(CancellationToken ct)
    {
        OrigemMercadoria orig = default;
        string csosnTexto = string.Empty;
        int modBCST = 0;
        decimal? pMVAST = null, pRedBCST = null;
        decimal vBCST = 0, pICMSST = 0, vICMSST = 0;
        decimal? vBCFCPST = null, pFCPST = null, vFCPST = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CSOSN": csosnTexto = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "modBCST": modBCST = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pMVAST": pMVAST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBCST": pRedBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCST": vBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMSST": pICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSST": vICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCFCPST": vBCFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCPST": pFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPST": vFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new IcmsSN202
        {
            Orig = orig,
            CSOSN = Csosn.Create(csosnTexto),
            ModBCST = modBCST,
            PMVAST = pMVAST, PRedBCST = pRedBCST,
            VBCST = vBCST, PICMSST = pICMSST, VICMSST = vICMSST,
            VBCFCPST = vBCFCPST, PFCPST = pFCPST, VFCPST = vFCPST,
        };
    }

    private async Task<IcmsSN500> ReadIcmsSN500Async(CancellationToken ct)
    {
        OrigemMercadoria orig = default;
        string csosnTexto = string.Empty;
        decimal? vBCSTRet = null, pST = null, vICMSSubstituto = null, vICMSSTRet = null;
        decimal? vBCFCPSTRet = null, pFCPSTRet = null, vFCPSTRet = null;
        decimal? pRedBCEfet = null, vBCEfet = null, pICMSEfet = null, vICMSEfet = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CSOSN": csosnTexto = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "vBCSTRet": vBCSTRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pST": pST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSSubstituto": vICMSSubstituto = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSSTRet": vICMSSTRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCFCPSTRet": vBCFCPSTRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCPSTRet": pFCPSTRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPSTRet": vFCPSTRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBCEfet": pRedBCEfet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCEfet": vBCEfet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMSEfet": pICMSEfet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSEfet": vICMSEfet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new IcmsSN500
        {
            Orig = orig,
            CSOSN = Csosn.Create(csosnTexto),
            VBCSTRet = vBCSTRet, PST = pST, VICMSSubstituto = vICMSSubstituto, VICMSSTRet = vICMSSTRet,
            VBCFCPSTRet = vBCFCPSTRet, PFCPSTRet = pFCPSTRet, VFCPSTRet = vFCPSTRet,
            PRedBCEfet = pRedBCEfet, VBCEfet = vBCEfet, PICMSEfet = pICMSEfet, VICMSEfet = vICMSEfet,
        };
    }

    private async Task<IcmsSN900> ReadIcmsSN900Async(CancellationToken ct)
    {
        // orig é opcional (minOccurs="0") nesta variante; ausente → default Nacional(0).
        OrigemMercadoria orig = default;
        string csosnTexto = string.Empty;
        int? modBC = null, modBCST = null;
        decimal? vBC = null, pRedBC = null, pICMS = null, vICMS = null;
        decimal? pMVAST = null, pRedBCST = null;
        decimal? vBCST = null, pICMSST = null, vICMSST = null;
        decimal? vBCFCPST = null, pFCPST = null, vFCPST = null;
        decimal? pCredSN = null, vCredICMSSN = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CSOSN": csosnTexto = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "modBC": modBC = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBC": vBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBC": pRedBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMS": pICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMS": vICMS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "modBCST": modBCST = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pMVAST": pMVAST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBCST": pRedBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCST": vBCST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMSST": pICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSST": vICMSST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCFCPST": vBCFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCPST": pFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPST": vFCPST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pCredSN": pCredSN = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vCredICMSSN": vCredICMSSN = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new IcmsSN900
        {
            Orig = orig,
            CSOSN = Csosn.Create(csosnTexto),
            ModBC = modBC, VBC = vBC, PRedBC = pRedBC, PICMS = pICMS, VICMS = vICMS,
            ModBCST = modBCST, PMVAST = pMVAST, PRedBCST = pRedBCST,
            VBCST = vBCST, PICMSST = pICMSST, VICMSST = vICMSST,
            VBCFCPST = vBCFCPST, PFCPST = pFCPST, VFCPST = vFCPST,
            PCredSN = pCredSN, VCredICMSSN = vCredICMSSN,
        };
    }

    private async Task<IcmsST> ReadIcmsSTAsync(CancellationToken ct)
    {
        OrigemMercadoria orig = default; string cstTributacao = string.Empty;
        decimal vBCSTRet = 0, vICMSSTRet = 0, vBCSTDest = 0, vICMSSTDest = 0;
        decimal? pST = null, vICMSSubstituto = null;
        decimal? vBCFCPSTRet = null, pFCPSTRet = null, vFCPSTRet = null;
        decimal? pRedBCEfet = null, vBCEfet = null, pICMSEfet = null, vICMSEfet = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "orig": orig = ParseEnum<OrigemMercadoria>(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "CST": cstTributacao = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "vBCSTRet": vBCSTRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pST": pST = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSSubstituto": vICMSSubstituto = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSSTRet": vICMSSTRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCFCPSTRet": vBCFCPSTRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pFCPSTRet": pFCPSTRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vFCPSTRet": vFCPSTRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCSTDest": vBCSTDest = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSSTDest": vICMSSTDest = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pRedBCEfet": pRedBCEfet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vBCEfet": vBCEfet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "pICMSEfet": pICMSEfet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vICMSEfet": vICMSEfet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        Cst cst = CombinarCst(orig, cstTributacao);

        return new IcmsST
        {
            Orig = orig, CST = cst,
            VBCSTRet = vBCSTRet, PST = pST, VICMSSubstituto = vICMSSubstituto, VICMSSTRet = vICMSSTRet,
            VBCFCPSTRet = vBCFCPSTRet, PFCPSTRet = pFCPSTRet, VFCPSTRet = vFCPSTRet,
            VBCSTDest = vBCSTDest, VICMSSTDest = vICMSSTDest,
            PRedBCEfet = pRedBCEfet, VBCEfet = vBCEfet, PICMSEfet = pICMSEfet, VICMSEfet = vICMSEfet,
        };
    }
}
