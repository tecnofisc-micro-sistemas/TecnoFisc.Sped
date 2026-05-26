using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe.Parser;

/// <summary>
/// Leitores de tributos não-ICMS: IPI (slice 14.4); PIS, COFINS, II, ISSQN entram em slices posteriores.
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
}
