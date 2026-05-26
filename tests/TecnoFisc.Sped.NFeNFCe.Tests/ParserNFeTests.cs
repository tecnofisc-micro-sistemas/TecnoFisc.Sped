namespace TecnoFisc.Sped.NFeNFCe.Tests;

public class ParserNFeTests
{
    [Fact]
    public void Default_Ctor_Uses_Default_Options()
    {
        var parser = new ParserNFe();

        parser.Options.Should().BeSameAs(ParserNFeOptions.Default);
    }

    [Fact]
    public void Ctor_Stores_Provided_Options()
    {
        var opcoes = new ParserNFeOptions { Strict = true };

        var parser = new ParserNFe(opcoes);

        parser.Options.Should().BeSameAs(opcoes);
    }

    [Fact]
    public void Ctor_Null_Options_Throws()
    {
        Action act = () => _ = new ParserNFe(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ReadAsync_Null_Stream_Throws()
    {
        var parser = new ParserNFe();

        Func<Task> act = () => parser.ReadAsync(null!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // Esqueleto da slice 14.2: o corpo de desserialização chega na 14.3 e este teste será
    // substituído por testes E2E de leitura real.
    [Fact]
    public async Task ReadAsync_Valid_Stream_NotImplemented_Until_14_3()
    {
        var parser = new ParserNFe();
        using var stream = new MemoryStream([1, 2, 3]);

        Func<Task> act = () => parser.ReadAsync(stream, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<NotImplementedException>();
    }
}
