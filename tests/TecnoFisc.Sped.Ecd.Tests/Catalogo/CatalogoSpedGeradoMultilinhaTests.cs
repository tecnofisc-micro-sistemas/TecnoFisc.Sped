using TecnoFisc.Sped.Ecd.Generated;

namespace TecnoFisc.Sped.Ecd.Tests.Catalogo;

/// <summary>
/// Garante que o catálogo gerado em compile-time (<see cref="CatalogoSpedGerado"/>, usado por
/// <c>new ParserEcd()</c>) propaga as flags de campo que antes só existiam no catálogo reflexivo:
/// <c>TokenFimArquivo</c>/<c>CampoArquivo</c> (registros multi-linha J800/J801) e <c>CapturaTudo</c>
/// (campo variádico RZ_CONT do I550/I555). O source generator não as emitia — gap que fazia o
/// parser de produção divergir do reflexivo (issue #498).
/// </summary>
public sealed class CatalogoSpedGeradoMultilinhaTests
{
    private static readonly CatalogoSpedGerado _gerado = new();

    [Fact]
    public void Gerado_J800_EhMultilinhaComTokenECampoArquivo()
    {
        _gerado.TentarObter("J800".AsSpan(), out var meta).Should().BeTrue();

        meta!.TokenFimArquivo.Should().Be("J800FIM");
        meta.EhMultilinha.Should().BeTrue();
        meta.Campos.Single(c => c.Nome == "ArqRtf").CampoArquivo.Should().BeTrue();
    }

    [Fact]
    public void Gerado_J801_EhMultilinhaComTokenECampoArquivo()
    {
        _gerado.TentarObter("J801".AsSpan(), out var meta).Should().BeTrue();

        meta!.TokenFimArquivo.Should().Be("J801FIM");
        meta.EhMultilinha.Should().BeTrue();
        meta.Campos.Single(c => c.Nome == "ArqRtf").CampoArquivo.Should().BeTrue();
    }

    [Fact]
    public void Gerado_I550_CampoVariadicoCapturaTudo()
    {
        _gerado.TentarObter("I550".AsSpan(), out var meta).Should().BeTrue();

        meta!.Campos.Single(c => c.Nome == "RzCont").CapturaTudo.Should().BeTrue();
    }

    [Fact]
    public void Gerado_RegistroComum_NaoEhMultilinha()
    {
        _gerado.TentarObter("J005".AsSpan(), out var meta).Should().BeTrue();

        meta!.TokenFimArquivo.Should().BeNull();
        meta.EhMultilinha.Should().BeFalse();
        meta.Campos.Should().OnlyContain(c => !c.CampoArquivo && !c.CapturaTudo);
    }
}
