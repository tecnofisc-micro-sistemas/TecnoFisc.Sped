namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Versionamento;

public sealed class LayoutEfdIcmsIpiTests
{
    [Fact]
    public void Enum_ContemDezesseteVersoesCobrindoJanelaFiscal()
    {
        var valores = Enum.GetValues<LayoutEfdIcmsIpi>();

        valores.Should().HaveCount(17);
    }

    [Theory]
    [InlineData(LayoutEfdIcmsIpi.V306, 306)]
    [InlineData(LayoutEfdIcmsIpi.V307, 307)]
    [InlineData(LayoutEfdIcmsIpi.V308, 308)]
    [InlineData(LayoutEfdIcmsIpi.V309, 309)]
    [InlineData(LayoutEfdIcmsIpi.V310, 310)]
    [InlineData(LayoutEfdIcmsIpi.V311, 311)]
    [InlineData(LayoutEfdIcmsIpi.V312, 312)]
    [InlineData(LayoutEfdIcmsIpi.V313, 313)]
    [InlineData(LayoutEfdIcmsIpi.V314, 314)]
    [InlineData(LayoutEfdIcmsIpi.V315, 315)]
    [InlineData(LayoutEfdIcmsIpi.V316, 316)]
    [InlineData(LayoutEfdIcmsIpi.V317, 317)]
    [InlineData(LayoutEfdIcmsIpi.V318, 318)]
    [InlineData(LayoutEfdIcmsIpi.V319, 319)]
    [InlineData(LayoutEfdIcmsIpi.V320, 320)]
    [InlineData(LayoutEfdIcmsIpi.V321, 321)]
    [InlineData(LayoutEfdIcmsIpi.V322, 322)]
    public void ValorNumerico_CodificaVersaoSemPonto(LayoutEfdIcmsIpi versao, int esperado)
    {
        ((int)versao).Should().Be(esperado);
    }

    [Fact]
    public void Valores_SaoStrictlyIncrementaisDeBaselineAteVigente()
    {
        var ordenados = Enum.GetValues<LayoutEfdIcmsIpi>().Cast<int>().ToArray();

        ordenados.Should().BeInAscendingOrder();
        ordenados[0].Should().Be(306, "V306 é o baseline da janela fiscal de 5 anos");
        ordenados[^1].Should().Be(322, "V322 é a versão vigente em 2026-05");
    }

    [Fact]
    public void Comparacao_PermiteUsoAritmeticoParaFiltroDeCampos()
    {
        int versaoArquivo = (int)LayoutEfdIcmsIpi.V311;

        (versaoArquivo >= (int)LayoutEfdIcmsIpi.V310).Should().BeTrue();
        (versaoArquivo >= (int)LayoutEfdIcmsIpi.V312).Should().BeFalse();
    }
}
