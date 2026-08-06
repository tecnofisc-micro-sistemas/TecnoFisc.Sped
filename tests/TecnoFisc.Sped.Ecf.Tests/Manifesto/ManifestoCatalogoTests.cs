using System.Text.Json.Nodes;

using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Txt.Engine.Catalogo;

namespace TecnoFisc.Sped.Ecf.Tests.Manifesto;

public sealed class ManifestoCatalogoTests
{
    [Fact]
    public void Manifesto_Tem180CodigosUnicosEmOrdemCanonica()
    {
        var manifesto = ManifestoEcf.Carregar();
        var codigos = manifesto.Registros.Select(registro => registro.Code).ToArray();

        codigos.Should().HaveCount(180);
        codigos.Should().OnlyHaveUniqueItems();
        codigos.Should().Equal(manifesto.CodigosCanonicos);
    }

    [Fact]
    public void Catalogo_NaoContemCodigoOuCampoForaDoManifesto()
    {
        AssertRegistroEcf.CatalogMatchesManifest();
        AssertRegistroEcf.CodesAreImplemented("0000");
    }

    [Fact]
    public void Parse_ManifestoComCodigoDuplicado_ApontaCodigoEPosicoes()
    {
        var (manifesto, schema) = LerArtefatosCopiados();
        var registros = JsonNode.Parse(manifesto)!.AsArray();
        registros[1]!["code"] = registros[0]!["code"]!.GetValue<string>();

        var act = () => ManifestoEcf.Parse(registros.ToJsonString(), schema);

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*duplicado*0000*posições 1 e 2*");
    }

    [Fact]
    public void Parse_ManifestoReordenado_ApontaPrimeiraDivergencia()
    {
        var (manifesto, schema) = LerArtefatosCopiados();
        var registros = JsonNode.Parse(manifesto)!.AsArray();
        var primeiro = registros[0]!.DeepClone();
        var segundo = registros[1]!.DeepClone();
        registros[0] = segundo;
        registros[1] = primeiro;

        var act = () => ManifestoEcf.Parse(registros.ToJsonString(), schema);

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*ordem canônica*posição 1*esperado '0000'*encontrado '0001'*");
    }

    [Fact]
    public void Parse_ManifestoComCodigoDesconhecido_ApontaCodigoForaDoSchema()
    {
        var (manifesto, schema) = LerArtefatosCopiados();
        var registros = JsonNode.Parse(manifesto)!.AsArray();
        registros[0]!["code"] = "ZZZZ";

        var act = () => ManifestoEcf.Parse(registros.ToJsonString(), schema);

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*código 'ZZZZ'*não existe*schema*");
    }

    [Fact]
    public void Parse_SchemaDeOutraVersao_ApontaDraftEsperado()
    {
        var (manifesto, schema) = LerArtefatosCopiados();
        var schemaNode = JsonNode.Parse(schema)!.AsObject();
        schemaNode["$schema"] = "https://json-schema.org/draft/2019-09/schema";

        var act = () => ManifestoEcf.Parse(manifesto, schemaNode.ToJsonString());

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*Draft 2020-12*");
    }

    [Fact]
    public void CodesAreImplemented_CodigoConhecidoMasAusente_ApontaFaltaNoCatalogo()
    {
        var act = () => AssertRegistroEcf.CodesAreImplemented("K001");

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*ausentes do catálogo*K001*");
    }

    [Fact]
    public void CodesAreImplemented_CodigoForaDoManifesto_ApontaCodigoDesconhecido()
    {
        var act = () => AssertRegistroEcf.CodesAreImplemented("ZZZZ");

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*fora do manifesto*ZZZZ*");
    }

    [Fact]
    public void MetadataMatchesManifest_TamanhoDivergente_ApontaRegistroCampoEValores()
    {
        var catalogo = new CatalogoSpedGerado();
        catalogo.TentarObter("0000", out var original).Should().BeTrue();
        var campos = original!.Campos
            .Select((campo, indice) => new MetadadosCampo(
                campo.Nome,
                campo.Ordem,
                campo.Tipo,
                indice == 0 ? campo.Tamanho + 1 : campo.Tamanho,
                campo.Decimais,
                campo.Obrigatorio,
                campo.Formato,
                campo.Definidor,
                campo.Serializar,
                campo.DesdeVersao,
                campo.CapturaTudo,
                campo.CampoArquivo))
            .ToArray();
        var divergente = new MetadadosRegistro(
            original.Codigo,
            original.Nivel,
            original.Bloco,
            original.TipoCSharp,
            original.Fabrica,
            campos,
            original.IntroduzidoEm,
            original.DescontinuadoEm,
            original.TokenFimArquivo);

        var act = () => AssertRegistroEcf.MetadataMatchesManifest(divergente);

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*registro 0000, campo nº 2 NOME_ESC*tamanho esperado '4', encontrado '5'*");
    }

    [Fact]
    public void ConformsToManifest_OcorrenciaDivergente_ApontaValorNormativo()
    {
        var act = () => AssertRegistroEcf.ConformsToManifest(
            new Registro0000(),
            "0000",
            "0:N");

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*registro 0000: ocorrência esperada '1:1', informada '0:N'*");
    }

    private static (string Manifesto, string Schema) LerArtefatosCopiados()
    {
        string diretorio = Path.Combine(AppContext.BaseDirectory, "Manifesto");
        return (
            File.ReadAllText(Path.Combine(diretorio, "layout-12-manifest.json")),
            File.ReadAllText(Path.Combine(diretorio, "layout-12-manifest.schema.json")));
    }
}
