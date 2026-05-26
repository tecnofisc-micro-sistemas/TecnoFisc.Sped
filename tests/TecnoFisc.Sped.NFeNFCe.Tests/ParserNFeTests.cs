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

    [Fact]
    public async Task ReadNFeAsync_Null_Stream_Throws()
    {
        var parser = new ParserNFe();

        Func<Task> act = () => parser.ReadNFeAsync(null!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ReadNFeAsync_Documento_Sem_InfNFe_Throws()
    {
        var parser = new ParserNFe();
        using var stream = new MemoryStream("<qualquer><coisa>1</coisa></qualquer>"u8.ToArray());

        Func<Task> act = () => parser.ReadNFeAsync(stream, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<FormatException>();
    }
}
