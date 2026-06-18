using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.Txt.Engine.Parser;

/// <summary>
/// Sniffer TXT fiscal opt-in. Use <see cref="SnifferSped"/> quando bastar identificar
/// projeto/versao; use este tipo quando tambem precisar de CNPJ e periodo.
/// </summary>
public static class SnifferSpedFiscal
{
    public static async ValueTask<MetadadosFiscaisArquivoSped> IdentificarAsync(
        Stream entrada,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);

        var identificacao = await SnifferSped.IdentificarAsync(entrada, cancelamento)
            .ConfigureAwait(false);

        var campos = identificacao.PrimeiraLinha.Split('|');
        var (indiceDataInicial, indiceDataFinal, indiceCnpj) = ObterIndices(identificacao.Projeto);

        return new MetadadosFiscaisArquivoSped(
            identificacao,
            ExtrairCnpj(campos, indiceCnpj),
            ExtrairData(campos, indiceDataInicial),
            ExtrairData(campos, indiceDataFinal));
    }

    private static (int DataInicial, int DataFinal, int Cnpj) ObterIndices(ProjetoSped projeto)
        => projeto switch
        {
            ProjetoSped.EfdContribuicoes => (6, 7, 9),
            ProjetoSped.EfdIcmsIpi => (4, 5, 7),
            ProjetoSped.Ecd => (3, 4, 6),
            _ => (-1, -1, -1),
        };

    private static Cnpj? ExtrairCnpj(string[] campos, int indice)
        => TryGetCampo(campos, indice) is string valor
            && Cnpj.TentarCriar(valor.AsSpan(), out var cnpj)
                ? cnpj
                : null;

    private static DateOnly? ExtrairData(string[] campos, int indice)
        => TryGetCampo(campos, indice) is string valor
            && ParseadoresPrimitivos.TentarDataComFormato(valor.AsSpan(), "ddMMyyyy", out var data)
                ? data
                : null;

    private static string? TryGetCampo(string[] campos, int indice)
        => indice >= 0 && indice < campos.Length && campos[indice].Length > 0
            ? campos[indice]
            : null;
}
