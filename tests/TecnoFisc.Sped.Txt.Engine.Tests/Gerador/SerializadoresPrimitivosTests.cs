using TecnoFisc.Sped.Txt.Engine.Gerador;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Gerador;

public sealed class SerializadoresPrimitivosTests
{
    [Theory]
    [InlineData(0, "0")]
    [InlineData(42, "42")]
    [InlineData(-1, "-1")]
    public void Inteiro_FormataNaCulturaInvariante(int valor, string esperado)
        => SerializadoresPrimitivos.Inteiro(valor).Should().Be(esperado);

    [Fact]
    public void Longo_FormataAlemDeInt32()
        => SerializadoresPrimitivos.Longo(9876543210L).Should().Be("9876543210");

    [Theory]
    [InlineData(0d, 2, "0,00")]
    [InlineData(1500.75d, 2, "1500,75")]
    [InlineData(-12.5d, 2, "-12,50")]
    [InlineData(1234d, 0, "1234")]
    [InlineData(0.1d, 4, "0,1000")]
    public void DeDecimal_UsaVirgulaECasasFixas(double valor, int decimais, string esperado)
        => SerializadoresPrimitivos.DeDecimal((decimal)valor, decimais).Should().Be(esperado);

    [Fact]
    public void Data_FormatoDDMMYYYY()
        => SerializadoresPrimitivos.Data(new DateOnly(2025, 1, 1)).Should().Be("01012025");

    [Theory]
    [InlineData("ddMMyyyy", 2025, 1, 1, "01012025")]
    [InlineData("MMyyyy", 2025, 3, 15, "032025")]
    [InlineData("yyyyMM", 2025, 3, 15, "202503")]
    public void DataComFormato_AtendeAosFormatosSPED(string formato, int ano, int mes, int dia, string esperado)
        => SerializadoresPrimitivos.DataComFormato(new DateOnly(ano, mes, dia), formato).Should().Be(esperado);
}
