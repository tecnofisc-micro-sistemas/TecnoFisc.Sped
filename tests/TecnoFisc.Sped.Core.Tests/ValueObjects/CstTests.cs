namespace TecnoFisc.Sped.Core.Tests.ValueObjects;

public sealed class CstTests
{
    [Theory]
    [InlineData(TipoTributo.Icms, "060", "060")]
    [InlineData(TipoTributo.Icms, "000", "000")]
    [InlineData(TipoTributo.Ipi, "49", "49")]
    [InlineData(TipoTributo.Pis, "01", "01")]
    [InlineData(TipoTributo.Cofins, "99", "99")]
    public void Criar_ComCodigoValido_RetornaCst(TipoTributo tributo, string entrada, string esperado)
    {
        var cst = Cst.Create(entrada, tributo);

        cst.ToString().Should().Be(esperado);
        cst.Tributo.Should().Be(tributo);
    }

    [Theory]
    [InlineData(TipoTributo.Icms, "60")]    // ICMS exige 3 dígitos
    [InlineData(TipoTributo.Icms, "0600")]
    [InlineData(TipoTributo.Pis, "001")]    // PIS/COFINS exigem 2 dígitos
    [InlineData(TipoTributo.Pis, "1")]
    [InlineData(TipoTributo.Ipi, "")]
    public void Criar_ComCodigoInvalidoParaTributo_LancaFormatException(TipoTributo tributo, string entrada)
    {
        var act = () => Cst.Create(entrada, tributo);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Equals_DiferenteTributo_RetornaFalse()
    {
        var pis = Cst.Create("01", TipoTributo.Pis);
        var cofins = Cst.Create("01", TipoTributo.Cofins);

        pis.Should().NotBe(cofins);
    }

    [Fact]
    public void Equals_MesmoTributoMesmoCodigo_RetornaTrue()
    {
        var a = Cst.Create("060", TipoTributo.Icms);
        var b = Cst.Create("060", TipoTributo.Icms);

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
