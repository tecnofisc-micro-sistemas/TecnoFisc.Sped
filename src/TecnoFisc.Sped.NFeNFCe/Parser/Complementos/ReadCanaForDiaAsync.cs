// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed partial class NFeXmlReader
{
    private async Task<CanaForDia> ReadCanaForDiaAsync(CancellationToken ct)
    {
        decimal? extraQtde = null;
        string? extraDia = _r.GetAttribute("dia");
        await _r.ConsumeChildrenAsync(async nome =>
        {
            switch (nome)
            {
                case "qtde": extraQtde = ParseDecimal(await _r.ReadTextAsync().ConfigureAwait(false)); return true;
                case "dia": extraDia = await _r.ReadTextAsync().ConfigureAwait(false); return true;
                default: return false;
            }
        }, _options.Strict, ct).ConfigureAwait(false);
        return new CanaForDia
        {
            Qtde = extraQtde,
            Dia = extraDia
        };
    }
}
