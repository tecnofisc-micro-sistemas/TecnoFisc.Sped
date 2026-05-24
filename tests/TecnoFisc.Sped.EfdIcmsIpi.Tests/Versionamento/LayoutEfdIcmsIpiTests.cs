namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Versionamento;

public sealed class LayoutEfdIcmsIpiTests
{
    [Fact]
    public void Enum_ContemVersoesDentroDaJanelaFiscalDeCincoAnos()
    {
        var valores = Enum.GetValues<LayoutEfdIcmsIpi>();

        valores.Should().Contain(LayoutEfdIcmsIpi.V015, "V015 é o limite inferior da janela fiscal (vigente a partir de 2021-01)");
        valores.Should().Contain(LayoutEfdIcmsIpi.V016, "V016 entra na cadeia incremental (Leiaute 2022)");
    }

    [Theory]
    [InlineData(LayoutEfdIcmsIpi.V015, 15)]
    [InlineData(LayoutEfdIcmsIpi.V016, 16)]
    public void ValorNumerico_CodificaCodVerDoRegistro0000(LayoutEfdIcmsIpi versao, int esperado)
    {
        ((int)versao).Should().Be(esperado);
    }

    [Fact]
    public void Baseline_EhLeiauteV015()
    {
        var ordenados = Enum.GetValues<LayoutEfdIcmsIpi>().Cast<int>().ToArray();

        ordenados.Should().BeInAscendingOrder();
        ordenados[0].Should().Be(15, "V015 é o leiaute mais antigo dentro da janela fiscal de 5 anos");
    }
}
