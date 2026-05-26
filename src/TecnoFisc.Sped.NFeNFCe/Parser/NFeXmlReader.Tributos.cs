using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe.Parser;

/// <summary>
/// Leitores de tributos não-ICMS: IPI, II e ISSQN (slice 14.4).
/// PIS/PISST/COFINS/COFINSST residem em <c>NFeXmlReader.PisCofins.cs</c>.
/// </summary>
internal sealed partial class NFeXmlReader
{
    // =========================================================================
    // IPI
    // =========================================================================

    private async Task<Ipi> ReadIpiAsync(CancellationToken ct)
    {
        Cnpj? cnpjProd = null;
        string? cSelo = null;
        long? qSelo = null;
        string cEnq = string.Empty;
        IpiTributacao? tributacao = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CNPJProd":
                    cnpjProd = Cnpj.Create(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "cSelo":
                    cSelo = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "qSelo":
                    qSelo = ParseLong(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "cEnq":
                    cEnq = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "IPITrib":
                    tributacao = await ReadIpiTribAsync(ct).ConfigureAwait(false);
                    return true;
                case "IPINT":
                    tributacao = await ReadIpiNtAsync(ct).ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Ipi
        {
            CNPJProd = cnpjProd,
            CSelo = cSelo,
            QSelo = qSelo,
            CEnq = cEnq,
            Tributacao = tributacao ?? throw new FormatException("IPI sem elemento IPITrib nem IPINT."),
        };
    }

    private async Task<IpiTrib> ReadIpiTribAsync(CancellationToken ct)
    {
        string cstTexto = string.Empty;
        decimal? vBC = null, pIPI = null;
        decimal? qUnid = null, vUnid = null;
        decimal vIPI = 0;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CST":
                    cstTexto = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "vBC":
                    vBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "pIPI":
                    pIPI = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "qUnid":
                    qUnid = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vUnid":
                    vUnid = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vIPI":
                    vIPI = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new IpiTrib
        {
            CST = Cst.Create(cstTexto, TipoTributo.Ipi),
            VBC = vBC,
            PIPI = pIPI,
            QUnid = qUnid,
            VUnid = vUnid,
            VIPI = vIPI,
        };
    }

    private async Task<IpiNt> ReadIpiNtAsync(CancellationToken ct)
    {
        string cstTexto = string.Empty;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CST":
                    cstTexto = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new IpiNt
        {
            CST = Cst.Create(cstTexto, TipoTributo.Ipi),
        };
    }

    // =========================================================================
    // II
    // =========================================================================

    /// <summary>
    /// Lê o grupo <c>II</c> — Imposto de Importação.
    /// Registro plano com quatro campos obrigatórios: <c>vBC</c>, <c>vDespAdu</c>, <c>vII</c> e <c>vIOF</c>.
    /// </summary>
    private async Task<Ii> ReadIiAsync(CancellationToken ct)
    {
        decimal vBC = 0, vDespAdu = 0, vII = 0, vIOF = 0;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vBC":
                    vBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vDespAdu":
                    vDespAdu = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vII":
                    vII = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vIOF":
                    vIOF = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Ii
        {
            VBC = vBC,
            VDespAdu = vDespAdu,
            VII = vII,
            VIOF = vIOF,
        };
    }

    // =========================================================================
    // ISSQN
    // =========================================================================

    /// <summary>
    /// Lê o grupo <c>ISSQN</c> — Imposto Sobre Serviços de Qualquer Natureza.
    /// Registro plano com campos obrigatórios e vários opcionais (minOccurs="0").
    /// </summary>
    private async Task<Issqn> ReadIssqnAsync(CancellationToken ct)
    {
        decimal vBC = 0, vAliq = 0, vISSQN = 0;
        CodigoMunicipioIbge cMunFG = default;
        string cListServ = string.Empty;
        int indISS = 0, indIncentivo = 0;
        decimal? vDeducao = null, vOutro = null, vDescIncond = null, vDescCond = null, vISSRet = null;
        string? cServico = null, nProcesso = null;
        CodigoMunicipioIbge? cMun = null;
        int? cPais = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vBC":
                    vBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vAliq":
                    vAliq = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vISSQN":
                    vISSQN = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "cMunFG":
                    cMunFG = CodigoMunicipioIbge.Create(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "cListServ":
                    cListServ = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "vDeducao":
                    vDeducao = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vOutro":
                    vOutro = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vDescIncond":
                    vDescIncond = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vDescCond":
                    vDescCond = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vISSRet":
                    vISSRet = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "indISS":
                    indISS = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "cServico":
                    cServico = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "cMun":
                    cMun = CodigoMunicipioIbge.Create(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "cPais":
                    cPais = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "nProcesso":
                    nProcesso = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "indIncentivo":
                    indIncentivo = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new Issqn
        {
            VBC = vBC,
            VAliq = vAliq,
            VISSQN = vISSQN,
            CMunFG = cMunFG,
            CListServ = cListServ,
            IndISS = indISS,
            IndIncentivo = indIncentivo,
            VDeducao = vDeducao,
            VOutro = vOutro,
            VDescIncond = vDescIncond,
            VDescCond = vDescCond,
            VISSRet = vISSRet,
            CServico = cServico,
            CMun = cMun,
            CPais = cPais,
            NProcesso = nProcesso,
        };
    }
}
