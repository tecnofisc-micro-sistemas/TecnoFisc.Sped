using System.Text.Json.Nodes;

namespace TecnoFisc.Sped.Ecf.Tests.Manifesto;

public sealed class ManifestoEcfAdversarialTests
{
    [Fact]
    public void Parse_RegistroFuturoComTipoBogus_ApontaCaminhoDoCampo()
    {
        var (manifesto, schema) = LerArtefatosCopiados();
        var registros = JsonNode.Parse(manifesto)!.AsArray();
        var registro = ObterRegistro(registros, "Y612");
        registro["fields"]!.AsArray()[1]!["type"] = "BOGUS";

        var act = () => ManifestoEcf.Parse(registros.ToJsonString(), schema);

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*schema*Y612*fields*type*");
    }

    [Fact]
    public void Parse_RegistroFuturoComObrigatoriedadeTalvez_ApontaCaminhoDoCampo()
    {
        var (manifesto, schema) = LerArtefatosCopiados();
        var registros = JsonNode.Parse(manifesto)!.AsArray();
        var registro = ObterRegistro(registros, "Y612");
        registro["fields"]!.AsArray()[1]!["required"] = "Talvez";

        var act = () => ManifestoEcf.Parse(registros.ToJsonString(), schema);

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*schema*Y612*fields*required*");
    }

    [Fact]
    public void Parse_RegistroFuturoComTituloVazio_ApontaCaminhoDoRegistro()
    {
        var (manifesto, schema) = LerArtefatosCopiados();
        var registros = JsonNode.Parse(manifesto)!.AsArray();
        ObterRegistro(registros, "Y612")["title"] = "";

        var act = () => ManifestoEcf.Parse(registros.ToJsonString(), schema);

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*schema*Y612*title*");
    }

    [Fact]
    public void Parse_RegistroFuturoComNomeDeCampoVazio_ApontaCaminhoDoCampo()
    {
        var (manifesto, schema) = LerArtefatosCopiados();
        var registros = JsonNode.Parse(manifesto)!.AsArray();
        var registro = ObterRegistro(registros, "Y612");
        registro["fields"]!.AsArray()[1]!["name"] = "";

        var act = () => ManifestoEcf.Parse(registros.ToJsonString(), schema);

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*schema*Y612*fields*name*");
    }

    [Fact]
    public void Parse_RegistroFuturoComPropriedadeExtra_ApontaAdditionalProperties()
    {
        var (manifesto, schema) = LerArtefatosCopiados();
        var registros = JsonNode.Parse(manifesto)!.AsArray();
        ObterRegistro(registros, "Y612")["unexpected"] = true;

        var act = () => ManifestoEcf.Parse(registros.ToJsonString(), schema);

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*schema*Y612*unexpected*");
    }

    [Fact]
    public void Parse_RegistroFuturoSemPropriedadeObrigatoria_ApontaRequired()
    {
        var (manifesto, schema) = LerArtefatosCopiados();
        var registros = JsonNode.Parse(manifesto)!.AsArray();
        ObterRegistro(registros, "Y612").Remove("title");

        var act = () => ManifestoEcf.Parse(registros.ToJsonString(), schema);

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*schema*Y612*title*");
    }

    [Fact]
    public void Parse_RegistroFuturoComTipoJsonErrado_ApontaCaminhoDaPropriedade()
    {
        var (manifesto, schema) = LerArtefatosCopiados();
        var registros = JsonNode.Parse(manifesto)!.AsArray();
        ObterRegistro(registros, "Y612")["pageStart"] = "562";

        var act = () => ManifestoEcf.Parse(registros.ToJsonString(), schema);

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*schema*Y612*pageStart*");
    }

    [Fact]
    public void Parse_RegistroFuturoComBlocoIncorreto_ApontaConstCanonica()
    {
        var (manifesto, schema) = LerArtefatosCopiados();
        var registros = JsonNode.Parse(manifesto)!.AsArray();
        ObterRegistro(registros, "Y612")["block"] = "X";

        var act = () => ManifestoEcf.Parse(registros.ToJsonString(), schema);

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*schema*Y612*block*");
    }

    [Fact]
    public void Parse_SchemaComPrefixItemsDeTipoInvalido_ApontaSchemaMalformado()
    {
        var (manifesto, schema) = LerArtefatosCopiados();
        var schemaNode = JsonNode.Parse(schema)!.AsObject();
        schemaNode["prefixItems"] = "invalido";

        var act = () => ManifestoEcf.Parse(manifesto, schemaNode.ToJsonString());

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*schema*prefixItems*");
    }

    private static JsonObject ObterRegistro(JsonArray registros, string codigo)
        => registros
            .Select(item => item!.AsObject())
            .Single(registro => registro["code"]!.GetValue<string>() == codigo);

    private static (string Manifesto, string Schema) LerArtefatosCopiados()
    {
        string diretorio = Path.Combine(AppContext.BaseDirectory, "Manifesto");
        return (
            File.ReadAllText(Path.Combine(diretorio, "layout-12-manifest.json")),
            File.ReadAllText(Path.Combine(diretorio, "layout-12-manifest.schema.json")));
    }
}
