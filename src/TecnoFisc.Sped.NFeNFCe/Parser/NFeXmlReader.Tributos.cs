using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe.Parser;

/// <summary>
/// Leitores de tributos não-ICMS: IPI e PIS (slice 14.4); COFINS, II, ISSQN entram em slices posteriores.
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
    // PIS
    // =========================================================================

    /// <summary>
    /// Lê o grupo <c>PIS</c> e despacha para a variante correta:
    /// <see cref="PisAliq"/>, <see cref="PisQtde"/>, <see cref="PisNt"/> ou <see cref="PisOutr"/>.
    /// </summary>
    private async Task<Pis> ReadPisAsync(CancellationToken ct)
    {
        Pis? variante = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "PISAliq":
                    variante = await ReadPisAliqAsync(ct).ConfigureAwait(false);
                    return true;
                case "PISQtde":
                    variante = await ReadPisQtdeAsync(ct).ConfigureAwait(false);
                    return true;
                case "PISNT":
                    variante = await ReadPisNtAsync(ct).ConfigureAwait(false);
                    return true;
                case "PISOutr":
                    variante = await ReadPisOutrAsync(ct).ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return variante ?? throw new FormatException("PIS sem variante reconhecida (PISAliq/PISQtde/PISNT/PISOutr).");
    }

    private async Task<PisAliq> ReadPisAliqAsync(CancellationToken ct)
    {
        string cstTexto = string.Empty;
        decimal vBC = 0, pPIS = 0, vPIS = 0;

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
                case "pPIS":
                    pPIS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vPIS":
                    vPIS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new PisAliq
        {
            CST = Cst.Create(cstTexto, TipoTributo.Pis),
            VBC = vBC,
            PPIS = pPIS,
            VPIS = vPIS,
        };
    }

    private async Task<PisQtde> ReadPisQtdeAsync(CancellationToken ct)
    {
        string cstTexto = string.Empty;
        decimal qBCProd = 0, vAliqProd = 0, vPIS = 0;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "CST":
                    cstTexto = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "qBCProd":
                    qBCProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vAliqProd":
                    vAliqProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vPIS":
                    vPIS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new PisQtde
        {
            CST = Cst.Create(cstTexto, TipoTributo.Pis),
            QBCProd = qBCProd,
            VAliqProd = vAliqProd,
            VPIS = vPIS,
        };
    }

    private async Task<PisNt> ReadPisNtAsync(CancellationToken ct)
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

        return new PisNt
        {
            CST = Cst.Create(cstTexto, TipoTributo.Pis),
        };
    }

    private async Task<PisOutr> ReadPisOutrAsync(CancellationToken ct)
    {
        string cstTexto = string.Empty;
        decimal? vBC = null, pPIS = null;
        decimal? qBCProd = null, vAliqProd = null;
        decimal vPIS = 0;

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
                case "pPIS":
                    pPIS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "qBCProd":
                    qBCProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vAliqProd":
                    vAliqProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vPIS":
                    vPIS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new PisOutr
        {
            CST = Cst.Create(cstTexto, TipoTributo.Pis),
            VBC = vBC,
            PPIS = pPIS,
            QBCProd = qBCProd,
            VAliqProd = vAliqProd,
            VPIS = vPIS,
        };
    }

    // =========================================================================
    // PISST
    // =========================================================================

    /// <summary>
    /// Lê o grupo <c>PISST</c> — PIS substituição tributária.
    /// Não possui CST; a inner choice (percentual/específico) é modelada com campos nullable.
    /// </summary>
    private async Task<PisSt> ReadPisStAsync(CancellationToken ct)
    {
        decimal? vBC = null, pPIS = null;
        decimal? qBCProd = null, vAliqProd = null;
        decimal vPIS = 0;
        int? indSomaPISST = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vBC":
                    vBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "pPIS":
                    pPIS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "qBCProd":
                    qBCProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vAliqProd":
                    vAliqProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vPIS":
                    vPIS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "indSomaPISST":
                    indSomaPISST = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new PisSt
        {
            VBC = vBC,
            PPIS = pPIS,
            QBCProd = qBCProd,
            VAliqProd = vAliqProd,
            VPIS = vPIS,
            IndSomaPISST = indSomaPISST,
        };
    }
}
