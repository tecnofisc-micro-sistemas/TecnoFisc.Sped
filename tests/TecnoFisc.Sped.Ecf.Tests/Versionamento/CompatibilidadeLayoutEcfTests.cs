using System.Text.Json.Nodes;

using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
namespace TecnoFisc.Sped.Ecf.Tests.Versionamento;

public sealed class CompatibilidadeLayoutEcfTests
{
    private static readonly IReadOnlyDictionary<string, int> IntroducoesEsperadas =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["Y750"] = 9,
            ["N605"] = 10,
            ["X360"] = 10,
            ["X365"] = 10,
            ["X366"] = 10,
            ["X370"] = 10,
            ["X371"] = 10,
            ["X375"] = 10,
            ["X485"] = 10,
            ["X451"] = 11,
            ["Y730"] = 12,
        };

    [Fact]
    public void CatalogoEManifesto_ModelamSomenteIntroducoesComprovadasNosLeiautes8A12()
    {
        var manifesto = ManifestoEcf.Carregar();
        var catalogo = new CatalogoSpedGerado().EnumerarRegistros().ToArray();

        manifesto.Registros
            .Where(registro => registro.IntroducedIn != 0)
            .ToDictionary(registro => registro.Code, registro => registro.IntroducedIn)
            .Should().BeEquivalentTo(IntroducoesEsperadas);
        catalogo
            .Where(registro => registro.IntroduzidoEm != 0)
            .ToDictionary(registro => registro.Codigo, registro => registro.IntroduzidoEm)
            .Should().BeEquivalentTo(IntroducoesEsperadas);

        manifesto.Registros
            .SelectMany(registro => registro.Fields.Select(campo =>
                (Registro: registro.Code, Campo: campo.Name, campo.SinceVersion)))
            .Where(item => item.SinceVersion != 0)
            .Should().BeEquivalentTo([
                ("0020", "POSSUI_CEBRAS", 10),
                ("0020", "CEBAS", 12),
            ]);

        var camposCatalogo = catalogo.Single(registro => registro.Codigo == "0020")
            .Campos.Where(campo => campo.DesdeVersao != 0)
            .Select(campo => (campo.Ordem, campo.DesdeVersao));
        camposCatalogo.Should().BeEquivalentTo([(31, 10), (32, 12)]);
    }

    [Fact]
    public void Introducoes_SaoIndisponiveisLogoAbaixoEDisponiveisNaVersaoDeclarada()
    {
        foreach (var (codigo, introduzidoEm) in IntroducoesEsperadas)
        {
            EstaDisponivel(introduzidoEm, introduzidoEm - 1)
                .Should().BeFalse($"{codigo} ainda não existia no leiaute anterior");
            EstaDisponivel(introduzidoEm, introduzidoEm)
                .Should().BeTrue($"{codigo} passa a existir no leiaute declarado");
        }

        EstaDisponivel(10, 9).Should().BeFalse();
        EstaDisponivel(10, 10).Should().BeTrue();
        EstaDisponivel(12, 11).Should().BeFalse();
        EstaDisponivel(12, 12).Should().BeTrue();
    }

    [Fact]
    public void MetadadoOmitido_PermaneceCompativelComLeiaute8()
    {
        var registro = ManifestoEcf.Carregar().Obter("0000");

        registro.IntroducedIn.Should().Be(0);
        registro.Fields.Should().OnlyContain(campo => campo.SinceVersion == 0);
        EstaDisponivel(registro.IntroducedIn, 8).Should().BeTrue();
    }

    [Theory]
    [InlineData("introducedIn", 7)]
    [InlineData("introducedIn", 13)]
    [InlineData("sinceVersion", 7)]
    [InlineData("sinceVersion", 13)]
    public void Schema_RejeitaVersaoForaDoIntervaloSuportado(string propriedade, int valor)
    {
        var (manifestoJson, schemaJson) = LerArtefatosCopiados();
        var registros = JsonNode.Parse(manifestoJson)!.AsArray();
        if (propriedade == "introducedIn")
            registros[0]![propriedade] = valor;
        else
            registros[0]!["fields"]![0]![propriedade] = valor;

        var act = () => ManifestoEcf.Parse(registros.ToJsonString(), schemaJson);

        act.Should().Throw<InvalidDataException>()
            .WithMessage($"*{propriedade}*");
    }

    [Fact]
    public void CatalogoAtual_NaoReintroduzRegistrosRemovidosNoLeiaute11()
    {
        string[] removidos = ["X291", "X300", "X305", "X310", "X320", "X325", "X330"];
        var codigos = new CatalogoSpedGerado().EnumerarRegistros()
            .Select(registro => registro.Codigo)
            .ToHashSet(StringComparer.Ordinal);

        codigos.Should().NotContain(removidos);
    }

    [Fact]
    public void CatalogoAtual_ExplicitaLimiteEstruturalDaMigracaoX450ParaX451()
    {
        var catalogo = new CatalogoSpedGerado();
        catalogo.TentarObter("X450", out var x450).Should().BeTrue();
        catalogo.TentarObter("X451", out var x451).Should().BeTrue();

        x450!.Campos.Select(campo => AssertRegistroEcf.CanonicalFieldName(campo.Nome))
            .Should().Equal("PAIS");
        x451!.Campos.Select(campo => AssertRegistroEcf.CanonicalFieldName(campo.Nome))
            .Should().Equal("CODIGO", "DESCRICAO", "VALOR");
    }

    private static bool EstaDisponivel(int introduzidoEm, int versao)
        => introduzidoEm == 0 || introduzidoEm <= versao;

    private static (string Manifesto, string Schema) LerArtefatosCopiados()
    {
        string diretorio = Path.Combine(AppContext.BaseDirectory, "Manifesto");
        return (
            File.ReadAllText(Path.Combine(diretorio, "layout-12-manifest.json")),
            File.ReadAllText(Path.Combine(diretorio, "layout-12-manifest.schema.json")));
    }
}
