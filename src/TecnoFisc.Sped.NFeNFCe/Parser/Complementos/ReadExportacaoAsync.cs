// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<Exportacao> ReadExportacaoAsync(CancellationToken ct)
    {
        string? extraUFSaidaPais = null;
        string? extraXLocExporta = null;
        string? extraXLocDespacho = null;
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "UFSaidaPais":
                    extraUFSaidaPais = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "xLocExporta":
                    extraXLocExporta = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                case "xLocDespacho":
                    extraXLocDespacho = await _r.ReadTextAsync().ConfigureAwait(false);
                    return true;
                default:
                    return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new Exportacao
        {
            UFSaidaPais = extraUFSaidaPais,
            XLocExporta = extraXLocExporta,
            XLocDespacho = extraXLocDespacho
        };
    }
}
