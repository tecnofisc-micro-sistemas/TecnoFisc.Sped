using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

public sealed class ParserEcfOpcoesTests
{
    [Fact]
    public void SemOpcoes_LigaVigenciaEValidacaoDeDominio()
    {
        var resolvidas = ParserEcf.ResolverOpcoes(ReadingOptions.Default);

        resolvidas.RespeitarVigenciaDoLeiaute.Should().BeTrue();
        resolvidas.ValidarDominioDeEnum.Should().BeTrue();
    }

    [Fact]
    public void OverrideExplicito_VenceOPadraoDoLeiaute()
    {
        var resolvidas = ParserEcf.ResolverOpcoes(new ReadingOptions
        {
            RespeitarVigenciaDoLeiaute = false,
            ValidarDominioDeEnum = false,
        });

        resolvidas.RespeitarVigenciaDoLeiaute.Should().BeFalse();
        resolvidas.ValidarDominioDeEnum.Should().BeFalse();
    }

    [Fact]
    public void ResolverOpcoes_PreservaOsDemaisCamposDoChamador()
    {
        var origem = new ReadingOptions
        {
            LenientLayout = true,
            LenientFieldParsing = true,
            RegistrosIgnorados = new HashSet<string>(StringComparer.Ordinal) { "Y800" },
            BlocosIgnorados = new HashSet<string>(StringComparer.Ordinal) { "X" },
        };

        var resolvidas = ParserEcf.ResolverOpcoes(origem);

        resolvidas.LenientLayout.Should().BeTrue();
        resolvidas.LenientFieldParsing.Should().BeTrue();
        resolvidas.RegistrosIgnorados.Should().BeEquivalentTo(["Y800"]);
        resolvidas.BlocosIgnorados.Should().BeEquivalentTo(["X"]);
    }
}
