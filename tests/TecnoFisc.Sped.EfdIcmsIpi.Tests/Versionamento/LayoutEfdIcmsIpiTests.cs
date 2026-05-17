namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Versionamento;

public sealed class LayoutEfdIcmsIpiTests
{
    [Fact]
    public void Enum_ContemApenasVersaoVigenteNaJanelaFiscal()
    {
        var valores = Enum.GetValues<LayoutEfdIcmsIpi>();

        valores.Should().ContainSingle().Which.Should().Be(LayoutEfdIcmsIpi.V015);
    }

    [Theory]
    [InlineData(LayoutEfdIcmsIpi.V015, 15)]
    public void ValorNumerico_CodificaCodVerDoRegistro0000(LayoutEfdIcmsIpi versao, int esperado)
    {
        ((int)versao).Should().Be(esperado);
    }

    [Fact]
    public void Baseline_EhLeiauteV015()
    {
        var ordenados = Enum.GetValues<LayoutEfdIcmsIpi>().Cast<int>().ToArray();

        ordenados.Should().BeInAscendingOrder();
        ordenados[0].Should().Be(15, "V015 é o leiaute vigente dentro da janela fiscal de 5 anos");
    }
}
