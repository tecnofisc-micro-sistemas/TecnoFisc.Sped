using System.Globalization;
using System.Text;

using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf.Tests.Versionamento;

/// <summary>
/// Monta o arquivo ECF mínimo usado pelos testes de versionamento — <c>0000</c> com o
/// <c>COD_VER</c> pedido, as linhas do caso e o <c>9999</c> de encerramento — e o lê com
/// <see cref="ParserEcf"/>. Existe para que os casos não repitam o cabeçalho nem esqueçam o
/// <c>CancellationToken</c> do teste.
/// </summary>
internal static class FixtureEcf
{
    /// <summary>Lê um arquivo cujo <c>COD_VER</c> é a versão numérica informada, em 4 dígitos.</summary>
    internal static Task<List<RegistroSped>> ReadAsync(int versao, params string[] linhas)
        => ReadAsync(versao.ToString("0000", CultureInfo.InvariantCulture), linhas);

    /// <summary>
    /// Lê um arquivo com o <c>COD_VER</c> literal informado — inclusive ilegível (<c>"ABCD"</c>,
    /// vazio, comprimento diferente de 4), que a sobrecarga numérica não consegue expressar.
    /// </summary>
    internal static async Task<List<RegistroSped>> ReadAsync(string codVer, params string[] linhas)
    {
        var arquivo = new StringBuilder()
            .Append(CultureInfo.InvariantCulture, $"|0000|LECF|{codVer}|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||")
            .Append("\r\n");
        foreach (string linha in linhas)
            arquivo.Append(linha).Append("\r\n");
        arquivo.Append("|9999|3|\r\n");

        var registros = new List<RegistroSped>();
        await using var stream = new MemoryStream(Encoding.Latin1.GetBytes(arquivo.ToString()));
        await foreach (var registro in new ParserEcf().ReadStreamingAsync(
            stream, TestContext.Current.CancellationToken))
        {
            registros.Add(registro);
        }

        return registros;
    }
}
