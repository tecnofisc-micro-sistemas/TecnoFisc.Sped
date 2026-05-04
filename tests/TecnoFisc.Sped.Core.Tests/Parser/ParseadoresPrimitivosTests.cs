using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.Core.Tests.Parser;

public sealed class ParseadoresPrimitivosTests
{
    [Theory]
    [InlineData("0", 0)]
    [InlineData("42", 42)]
    [InlineData("-7", -7)]
    public void Inteiro_ConverteComCulturaInvariante(string entrada, int esperado)
        => ParseadoresPrimitivos.Inteiro(entrada).Should().Be(esperado);

    [Fact]
    public void InteiroOuPadrao_QuandoVazio_RetornaPadrao()
        => ParseadoresPrimitivos.InteiroOuPadrao(string.Empty, padrao: 99).Should().Be(99);

    [Fact]
    public void Longo_ConverteValorAlemDeInt32()
        => ParseadoresPrimitivos.Longo("9876543210").Should().Be(9876543210L);

    [Theory]
    [InlineData("0", 0d)]
    [InlineData("0,00", 0d)]
    [InlineData("1500,75", 1500.75d)]
    [InlineData("1500.75", 1500.75d)]
    [InlineData("-12,5", -12.5d)]
    public void ParaDecimal_AceitaVirgulaEPonto(string entrada, double esperado)
        => ParseadoresPrimitivos.ParaDecimal(entrada).Should().Be((decimal)esperado);

    [Fact]
    public void ParaDecimal_QuandoVazio_LancaFormatException()
    {
        var act = () => ParseadoresPrimitivos.ParaDecimal(string.Empty);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void TentarDecimal_QuandoInvalido_RetornaFalse()
    {
        var ok = ParseadoresPrimitivos.TentarDecimal("abc", out var valor);

        ok.Should().BeFalse();
        valor.Should().Be(0m);
    }

    [Fact]
    public void Data_NoFormatoPadraoDDMMYYYY()
    {
        ParseadoresPrimitivos.Data("01012025").Should().Be(new DateOnly(2025, 1, 1));
        ParseadoresPrimitivos.Data("31122024").Should().Be(new DateOnly(2024, 12, 31));
    }

    [Theory]
    [InlineData("01012025", "ddMMyyyy", 2025, 1, 1)]
    [InlineData("032025", "MMyyyy", 2025, 3, 1)]
    [InlineData("202503", "yyyyMM", 2025, 3, 1)]
    public void DataComFormato_AceitaFormatosConhecidos(string entrada, string formato, int ano, int mes, int dia)
        => ParseadoresPrimitivos.DataComFormato(entrada, formato).Should().Be(new DateOnly(ano, mes, dia));

    [Fact]
    public void DataComFormato_QuandoTamanhoErrado_LancaFormatException()
    {
        var act = () => ParseadoresPrimitivos.DataComFormato("0101202", null);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void DataComFormato_QuandoCaractereNaoNumerico_LancaFormatException()
    {
        var act = () => ParseadoresPrimitivos.DataComFormato("01A12025", null);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void TentarDataComFormato_QuandoInvalido_RetornaFalse()
    {
        var ok = ParseadoresPrimitivos.TentarDataComFormato("99999999", null, out var valor);

        ok.Should().BeFalse();
        valor.Should().Be(default(DateOnly));
    }
}
