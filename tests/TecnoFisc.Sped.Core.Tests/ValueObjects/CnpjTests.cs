namespace TecnoFisc.Sped.Core.Tests.ValueObjects;

public sealed class CnpjTests
{
    [Theory]
    [InlineData("11222333000181")]
    [InlineData("11.222.333/0001-81")]
    [InlineData(" 11.222.333/0001-81 ")]
    public void Criar_ComValorValido_RetornaCnpj(string entrada)
    {
        var cnpj = Cnpj.Criar(entrada);

        cnpj.ToString().Should().Be("11222333000181");
        cnpj.ToStringFormatado().Should().Be("11.222.333/0001-81");
    }

    [Theory]
    [InlineData("11222333000182")]      // DV2 errado
    [InlineData("11222333000191")]      // DV1 errado
    [InlineData("1122233300018")]       // 13 dígitos
    [InlineData("112223330001811")]     // 15 dígitos
    [InlineData("00000000000000")]      // todos iguais
    [InlineData("11111111111111")]      // todos iguais
    [InlineData("1122233300018A")]      // caractere inválido
    [InlineData("")]
    public void Criar_ComValorInvalido_LancaFormatException(string entrada)
    {
        var act = () => Cnpj.Criar(entrada);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void TentarCriar_ComValorInvalido_RetornaFalse()
    {
        var resultado = Cnpj.TentarCriar("00000000000000", out var cnpj);

        resultado.Should().BeFalse();
        cnpj.Should().Be(default(Cnpj));
    }

    [Fact]
    public void Equals_ComMesmosDigitos_RetornaTrue()
    {
        var a = Cnpj.Criar("11222333000181");
        var b = Cnpj.Criar("11.222.333/0001-81");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void ConversaoImplicita_ParaString_RetornaFormaCanonica()
    {
        var cnpj = Cnpj.Criar("11.222.333/0001-81");

        string s = cnpj;

        s.Should().Be("11222333000181");
    }

    [Fact]
    public void Default_ToString_RetornaQuatorzeZeros()
    {
        Cnpj cnpj = default;

        cnpj.ToString().Should().Be("00000000000000");
    }
}
