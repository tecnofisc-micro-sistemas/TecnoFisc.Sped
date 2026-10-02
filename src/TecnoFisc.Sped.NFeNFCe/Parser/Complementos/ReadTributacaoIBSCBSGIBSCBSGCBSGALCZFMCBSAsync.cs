// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<TributacaoIBSCBSGIBSCBSGCBSGALCZFMCBS> ReadTributacaoIBSCBSGIBSCBSGCBSGALCZFMCBSAsync(CancellationToken ct)
    {
        string? extraTpALCZFMCBS = null;
        string? extraNProcSuframa = null;
        decimal? extraPAliqEfetRegCBS = null;
        decimal? extraVTribRegCBS = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "tpALCZFMCBS": extraTpALCZFMCBS = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "nProcSuframa": extraNProcSuframa = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                case "pAliqEfetRegCBS": extraPAliqEfetRegCBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "vTribRegCBS": extraVTribRegCBS = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new TributacaoIBSCBSGIBSCBSGCBSGALCZFMCBS
        {
            TpALCZFMCBS = extraTpALCZFMCBS,
            NProcSuframa = extraNProcSuframa,
            PAliqEfetRegCBS = extraPAliqEfetRegCBS,
            VTribRegCBS = extraVTribRegCBS
        };
    }
}
