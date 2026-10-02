// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<VeiculoProduto> ReadVeiculoProdutoAsync(CancellationToken ct)
    {
        string? extraTpOp = null;
        string? extraChassi = null;
        string? extraCCor = null;
        string? extraXCor = null;
        string? extraPot = null;
        string? extraCilin = null;
        string? extraPesoL = null;
        string? extraPesoB = null;
        string? extraNSerie = null;
        string? extraTpComb = null;
        string? extraNMotor = null;
        string? extraCMT = null;
        string? extraDist = null;
        string? extraAnoMod = null;
        string? extraAnoFab = null;
        string? extraTpPint = null;
        string? extraTpVeic = null;
        string? extraEspVeic = null;
        string? extraVIN = null;
        string? extraCondVeic = null;
        string? extraCMod = null;
        string? extraCCorDENATRAN = null;
        string? extraLota = null;
        string? extraTpRest = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "tpOp":
                    extraTpOp = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "chassi":
                    extraChassi = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "cCor":
                    extraCCor = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "xCor":
                    extraXCor = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "pot":
                    extraPot = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "cilin":
                    extraCilin = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "pesoL":
                    extraPesoL = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "pesoB":
                    extraPesoB = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "nSerie":
                    extraNSerie = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "tpComb":
                    extraTpComb = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "nMotor":
                    extraNMotor = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "CMT":
                    extraCMT = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "dist":
                    extraDist = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "anoMod":
                    extraAnoMod = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "anoFab":
                    extraAnoFab = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "tpPint":
                    extraTpPint = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "tpVeic":
                    extraTpVeic = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "espVeic":
                    extraEspVeic = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "VIN":
                    extraVIN = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "condVeic":
                    extraCondVeic = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "cMod":
                    extraCMod = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "cCorDENATRAN":
                    extraCCorDENATRAN = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "lota":
                    extraLota = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "tpRest":
                    extraTpRest = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new VeiculoProduto
        {
            TpOp = extraTpOp,
            Chassi = extraChassi,
            CCor = extraCCor,
            XCor = extraXCor,
            Pot = extraPot,
            Cilin = extraCilin,
            PesoL = extraPesoL,
            PesoB = extraPesoB,
            NSerie = extraNSerie,
            TpComb = extraTpComb,
            NMotor = extraNMotor,
            CMT = extraCMT,
            Dist = extraDist,
            AnoMod = extraAnoMod,
            AnoFab = extraAnoFab,
            TpPint = extraTpPint,
            TpVeic = extraTpVeic,
            EspVeic = extraEspVeic,
            VIN = extraVIN,
            CondVeic = extraCondVeic,
            CMod = extraCMod,
            CCorDENATRAN = extraCCorDENATRAN,
            Lota = extraLota,
            TpRest = extraTpRest
        };
    }
}
