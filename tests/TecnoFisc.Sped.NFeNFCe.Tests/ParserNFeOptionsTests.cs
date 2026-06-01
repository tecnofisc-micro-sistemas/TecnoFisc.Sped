namespace TecnoFisc.Sped.NFeNFCe.Tests;

public class ParserNFeOptionsTests
{
    [Fact]
    public void Default_Has_Expected_Values()
    {
        var opcoes = ParserNFeOptions.Default;

        opcoes.Strict.Should().BeFalse();
        opcoes.ValidateChecksums.Should().BeTrue();
        opcoes.Parallelism.Should().Be(Environment.ProcessorCount);
        opcoes.FailFast.Should().BeTrue();
    }

    [Fact]
    public void Init_Overrides_Are_Applied()
    {
        var opcoes = new ParserNFeOptions
        {
            Strict = true,
            ValidateChecksums = false,
            Parallelism = 4,
            FailFast = false,
        };

        opcoes.Strict.Should().BeTrue();
        opcoes.ValidateChecksums.Should().BeFalse();
        opcoes.Parallelism.Should().Be(4);
        opcoes.FailFast.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Parallelism_Below_One_Throws(int invalido)
    {
        Action act = () => _ = new ParserNFeOptions { Parallelism = invalido };

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
