using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe.Parser;

/// <summary>
/// Leitores de PIS, PISST, COFINS e COFINSST (slice 14.4 — tributos federais cumulativos/não-cumulativos).
/// </summary>
internal sealed partial class NFeXmlReader
{
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

    // =========================================================================
    // COFINS
    // =========================================================================

    /// <summary>
    /// Lê o grupo <c>COFINS</c> e despacha para a variante correta:
    /// <see cref="CofinsAliq"/>, <see cref="CofinsQtde"/>, <see cref="CofinsNt"/> ou <see cref="CofinsOutr"/>.
    /// </summary>
    private async Task<Cofins> ReadCofinsAsync(CancellationToken ct)
    {
        Cofins? variante = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "COFINSAliq":
                    variante = await ReadCofinsAliqAsync(ct).ConfigureAwait(false);
                    return true;
                case "COFINSQtde":
                    variante = await ReadCofinsQtdeAsync(ct).ConfigureAwait(false);
                    return true;
                case "COFINSNT":
                    variante = await ReadCofinsNtAsync(ct).ConfigureAwait(false);
                    return true;
                case "COFINSOutr":
                    variante = await ReadCofinsOutrAsync(ct).ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return variante ?? throw new FormatException("COFINS sem variante reconhecida (COFINSAliq/COFINSQtde/COFINSNT/COFINSOutr).");
    }

    private async Task<CofinsAliq> ReadCofinsAliqAsync(CancellationToken ct)
    {
        string cstTexto = string.Empty;
        decimal vBC = 0, pCOFINS = 0, vCOFINS = 0;

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
                case "pCOFINS":
                    pCOFINS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vCOFINS":
                    vCOFINS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new CofinsAliq
        {
            CST = Cst.Create(cstTexto, TipoTributo.Cofins),
            VBC = vBC,
            PCOFINS = pCOFINS,
            VCOFINS = vCOFINS,
        };
    }

    private async Task<CofinsQtde> ReadCofinsQtdeAsync(CancellationToken ct)
    {
        string cstTexto = string.Empty;
        decimal qBCProd = 0, vAliqProd = 0, vCOFINS = 0;

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
                case "vCOFINS":
                    vCOFINS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new CofinsQtde
        {
            CST = Cst.Create(cstTexto, TipoTributo.Cofins),
            QBCProd = qBCProd,
            VAliqProd = vAliqProd,
            VCOFINS = vCOFINS,
        };
    }

    private async Task<CofinsNt> ReadCofinsNtAsync(CancellationToken ct)
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

        return new CofinsNt
        {
            CST = Cst.Create(cstTexto, TipoTributo.Cofins),
        };
    }

    private async Task<CofinsOutr> ReadCofinsOutrAsync(CancellationToken ct)
    {
        string cstTexto = string.Empty;
        decimal? vBC = null, pCOFINS = null;
        decimal? qBCProd = null, vAliqProd = null;
        decimal vCOFINS = 0;

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
                case "pCOFINS":
                    pCOFINS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "qBCProd":
                    qBCProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vAliqProd":
                    vAliqProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vCOFINS":
                    vCOFINS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new CofinsOutr
        {
            CST = Cst.Create(cstTexto, TipoTributo.Cofins),
            VBC = vBC,
            PCOFINS = pCOFINS,
            QBCProd = qBCProd,
            VAliqProd = vAliqProd,
            VCOFINS = vCOFINS,
        };
    }

    // =========================================================================
    // COFINSST
    // =========================================================================

    /// <summary>
    /// Lê o grupo <c>COFINSST</c> — COFINS substituição tributária.
    /// Não possui CST; a inner choice (percentual/específico) é modelada com campos nullable.
    /// </summary>
    private async Task<CofinsSt> ReadCofinsStAsync(CancellationToken ct)
    {
        decimal? vBC = null, pCOFINS = null;
        decimal? qBCProd = null, vAliqProd = null;
        decimal vCOFINS = 0;
        int? indSomaCOFINSST = null;

        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "vBC":
                    vBC = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "pCOFINS":
                    pCOFINS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "qBCProd":
                    qBCProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vAliqProd":
                    vAliqProd = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "vCOFINS":
                    vCOFINS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                case "indSomaCOFINSST":
                    indSomaCOFINSST = ParseInt(await _r.ReadTextAsync().ConfigureAwait(false));
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);

        return new CofinsSt
        {
            VBC = vBC,
            PCOFINS = pCOFINS,
            QBCProd = qBCProd,
            VAliqProd = vAliqProd,
            VCOFINS = vCOFINS,
            IndSomaCOFINSST = indSomaCOFINSST,
        };
    }
}
