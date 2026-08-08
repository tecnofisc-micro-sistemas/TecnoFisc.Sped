using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

/// <summary>
/// Prova que a sobrecarga de <c>ParserEcf.ParseLinha</c> que recebe <see cref="LayoutEcf"/> aplica
/// a vigência do leiaute informado, em paridade com <see cref="ParserEcf.ReadStreamingAsync"/>.
/// </summary>
public sealed class ParserEcfParseLinhaTests
{
    private static string Linha0020ComTrintaEDuasColunas()
    {
        var valores = new List<string> { "1", "1" };
        valores.AddRange(Enumerable.Repeat("N", 27));
        valores.Add("S");
        valores.Add("CEBAS-TESTE");
        return "|0020|" + string.Join('|', valores) + "|";
    }

    [Fact]
    public void ParseLinha_ComLeiaute9_NaoPreencheCampoIntroduzidoNo12()
    {
        var resultado = new ParserEcf().ParseLinha(Linha0020ComTrintaEDuasColunas(), LayoutEcf.V009);

        resultado.Sucesso.Should().BeTrue();
        ((Registro0020)resultado.Valor!).Cebas.Should().BeNull();
    }

    [Fact]
    public void ParseLinha_ComLeiaute12_PreencheCampoIntroduzidoNo12()
    {
        var resultado = new ParserEcf().ParseLinha(Linha0020ComTrintaEDuasColunas(), LayoutEcf.V012);

        ((Registro0020)resultado.Valor!).Cebas.Should().Be("CEBAS-TESTE");
    }
}
