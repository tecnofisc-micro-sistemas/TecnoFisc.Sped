using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

/// <summary>
/// Prova que a sobrecarga de <c>ParserEcf.ParseLinha</c> que recebe <see cref="LayoutEcf"/> aplica
/// a vigência do leiaute informado em paridade com <see cref="ParserEcf.ReadStreamingAsync"/>, nos
/// dois níveis: vigência de campo (o campo introduzido depois não recebe valor) e vigência de
/// registro (o registro introduzido depois devolve falha, com a mesma mensagem do streaming).
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

    /// <summary>Registro Y730 tem <c>IntroduzidoEm = V012</c>: não existe no leiaute 9.</summary>
    private const string LinhaY730 = "|Y730|0001|0001|01012025|01|11111111000191|DESTINATARIO|1234,56|";

    [Fact]
    public void ParseLinha_ComLeiaute9_FalhaNoRegistroIntroduzidoNo12()
    {
        var resultado = new ParserEcf().ParseLinha(LinhaY730, LayoutEcf.V009);

        resultado.Sucesso.Should().BeFalse();
        var erro = resultado.Erros.Should().ContainSingle().Which;
        erro.CodigoRegistro.Should().Be("Y730");
        erro.Mensagem.Should().Be("Registro posterior à versão declarada no 0000 (9).");
    }

    [Fact]
    public void ParseLinha_ComLeiaute12_AceitaRegistroIntroduzidoNo12()
    {
        var resultado = new ParserEcf().ParseLinha(LinhaY730, LayoutEcf.V012);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY730>();
    }

    /// <summary>
    /// A sobrecarga sem <c>LayoutEcf</c> continua sem aplicar vigência nenhuma — é o contrato
    /// documentado, e é o que separa "não informei versão" de "informei uma versão anterior".
    /// </summary>
    [Fact]
    public void ParseLinha_SemLeiaute_AceitaRegistroIntroduzidoNo12()
        => new ParserEcf().ParseLinha(LinhaY730).Sucesso.Should().BeTrue();
}
