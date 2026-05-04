namespace TecnoFisc.Sped.Core.Tests.ValueObjects;

public sealed class InscricaoEstadualTests
{
    [Theory]
    [InlineData("ISENTO", "ISENTO", true)]
    [InlineData("isento", "ISENTO", true)]
    [InlineData(" Isento ", "ISENTO", true)]
    [InlineData("12345678", "12345678", false)]
    [InlineData("123.456.789", "123456789", false)]
    [InlineData("AB1234567", "AB1234567", false)]
    [InlineData("ab1234567", "AB1234567", false)]
    public void Criar_ComValorValido_NormalizaResultado(string entrada, string esperado, bool isento)
    {
        var ie = InscricaoEstadual.Criar(entrada);

        ie.ToString().Should().Be(esperado);
        ie.EhIsento.Should().Be(isento);
    }

    [Theory]
    [InlineData("")]                     // vazio
    [InlineData("A")]                    // < 2
    [InlineData("123456789012345")]      // > 14
    [InlineData("12@456")]               // caractere proibido
    public void Criar_ComValorInvalido_LancaFormatException(string entrada)
    {
        var act = () => InscricaoEstadual.Criar(entrada);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Equals_ComMesmoValorNormalizado_RetornaTrue()
    {
        var a = InscricaoEstadual.Criar("123.456.789");
        var b = InscricaoEstadual.Criar("123456789");

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
