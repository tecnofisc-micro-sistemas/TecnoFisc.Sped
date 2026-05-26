namespace TecnoFisc.Sped.Core.Tests.ValueObjects;

public sealed class CodigoMunicipioIbgeTests
{
    [Fact]
    public void Criar_ComSeteDigitos_RetornaCodigo()
    {
        var mun = CodigoMunicipioIbge.Create("3168606");

        mun.ToString().Should().Be("3168606");
    }

    [Fact]
    public void CodigoUf_ExtraiDoisPrimeirosDigitos()
    {
        CodigoMunicipioIbge.Create("3168606").CodigoUf.Should().Be(31);
    }

    [Fact]
    public void Criar_AceitaCodigoExterior9999999()
    {
        var mun = CodigoMunicipioIbge.Create("9999999");

        mun.ToString().Should().Be("9999999");
    }

    [Theory]
    [InlineData("316860")]    // 6 dígitos
    [InlineData("31686066")]  // 8 dígitos
    [InlineData("316860X")]   // não-dígito
    public void Criar_ComTamanhoOuCharInvalido_LancaFormatException(string entrada)
    {
        var act = () => CodigoMunicipioIbge.Create(entrada);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Equality_PorValor()
    {
        CodigoMunicipioIbge.Create("3168606").Should().Be(CodigoMunicipioIbge.Create("3168606"));
    }
}
