namespace TecnoFisc.Sped.Core.Tests.ValueObjects;

public sealed class CpfTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    [InlineData(" 529.982.247-25 ")]
    public void Criar_ComValorValido_RetornaCpf(string entrada)
    {
        var cpf = Cpf.Criar(entrada);

        cpf.ToString().Should().Be("52998224725");
        cpf.ToStringFormatado().Should().Be("529.982.247-25");
    }

    [Theory]
    [InlineData("52998224726")]      // DV2 errado
    [InlineData("52998224715")]      // DV1 errado
    [InlineData("5299822472")]       // 10 dígitos
    [InlineData("529982247255")]     // 12 dígitos
    [InlineData("00000000000")]      // todos iguais
    [InlineData("11111111111")]      // todos iguais
    [InlineData("5299822472A")]      // caractere inválido
    [InlineData("")]
    public void Criar_ComValorInvalido_LancaFormatException(string entrada)
    {
        var act = () => Cpf.Criar(entrada);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void TentarCriar_ComValorInvalido_RetornaFalse()
    {
        var ok = Cpf.TentarCriar("00000000000", out var cpf);

        ok.Should().BeFalse();
        cpf.Should().Be(default(Cpf));
    }

    [Fact]
    public void Equals_ComMesmosDigitos_RetornaTrue()
    {
        var a = Cpf.Criar("52998224725");
        var b = Cpf.Criar("529.982.247-25");

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
