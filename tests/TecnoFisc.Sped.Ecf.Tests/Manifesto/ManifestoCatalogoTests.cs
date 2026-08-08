using System.Text.Json.Nodes;

using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Tests.Versionamento;
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

    /// <summary>
    /// Carrega o manifesto real (não sintético) contra o schema real usando o
    /// validador .NET (Json.Schema / JsonSchema.Net), que <c>ManifestoEcf.Parse</c>
    /// usa em produção. Existe porque o <c>pattern</c> de <c>field.name</c> no
    /// schema já divergiu de comportamento entre o <c>jsonschema</c> do Python
    /// (tooling em <c>tools/ecf-layout</c>) e o <c>JsonSchema.Net</c> do .NET: um
    /// <c>pattern</c> escrito com <c>\w</c>/<c>\W</c> passou nos 516 testes Python
    /// (semântica Unicode) e falhou aqui (semântica ASCII-only sob ECMAScript),
    /// rejeitando os 5 registros com nome de campo acentuado
    /// (C050, J050, W250, W300, 9100). Um teste só em Python nunca pegaria essa
    /// classe de divergência — só carregar o schema real pelo runtime .NET real
    /// pega. Se o schema voltar a usar uma classe de caractere dependente de
    /// runtime, este teste falha aqui, não só na CI de outra linguagem.
    /// </summary>
    [Fact]
    public void ManifestoReal_ValidaContraSchemaRealPeloValidadorNet()
    {
        var act = () => ManifestoEcf.Carregar();

        act.Should().NotThrow();
    }

    [Fact]
    public void NomeCanonico_PreservaDiacriticoSemDependerDeDecomposicaoUnicode()
    {
        AssertRegistroEcf.CanonicalFieldName("NOME_ESC").Should().Be("NOMEESC");
        AssertRegistroEcf.CanonicalFieldName("NomeEsc").Should().Be("NOMEESC");
        AssertRegistroEcf.CanonicalFieldName("CONTEÚDO").Should().Be("CONTEÚDO");
        AssertRegistroEcf.CanonicalFieldName("Conteúdo").Should().Be("CONTEÚDO");
        AssertRegistroEcf.CanonicalFieldName("CONTEUDO").Should().Be("CONTEUDO");
        AssertRegistroEcf.CanonicalFieldName("CONTEÚDO").Should().Be("CONTEUDO");
    }

    [Theory]
    [InlineData("C050", 5, "NÍVEL")]
    [InlineData("J050", 5, "NÍVEL")]
    [InlineData("W250", 10, "ENDEREÇO")]
    [InlineData("W250", 27, "OBSERVAÇÃO")]
    [InlineData("W300", 13, "OBSERVAÇÃO")]
    [InlineData("9100", 6, "CONTEÚDO")]
    public void CatalogosGeradoEReflexivo_EmitemNomeNormativoAcentuadoOrdinalExato(
        string codigo,
        int numeroCampo,
        string nomeEsperado)
    {
        var catalogo = new CatalogoSpedGerado();
        catalogo.TentarObter(codigo, out var registro).Should().BeTrue();
        var reflexivo = CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);
        reflexivo.TentarObter(codigo, out var registroReflexivo).Should().BeTrue();

        registro!.Campos[numeroCampo - 2].Nome.Should().Be(nomeEsperado);
        registroReflexivo!.Campos[numeroCampo - 2].Nome.Should().Be(nomeEsperado);
    }

    [Fact]
    public void Catalogo_CorrespondeExatamenteAos180RegistrosE17BlocosDoManifesto()
    {
        var manifesto = ManifestoEcf.Carregar();
        MetadadosRegistro[] catalogoCompleto = new CatalogoSpedGerado().EnumerarRegistros().ToArray();
        // O manifesto descreve só o leiaute 12 vigente: os sete registros removidos no leiaute 11
        // (DescontinuadoEm != 0) ficam de fora desta comparação de 1:1 — ver
        // Catalogo_SoDivergeDoManifestoNosSeteRemovidosNoLeiaute11 para a asserção que trava o
        // conjunto exato da divergência.
        MetadadosRegistro[] catalogo = catalogoCompleto
            .Where(registro => registro.DescontinuadoEm == 0)
            .ToArray();

        manifesto.Registros.Should().HaveCount(180).And.OnlyContain(registro => registro.Reviewed);
        catalogo.Should().HaveCount(180);
        catalogo.Select(registro => registro.Codigo)
            .Should().Equal(manifesto.Registros.Select(registro => registro.Code));
        ExtrairBlocosEmOrdem(catalogo.Select(registro => registro.Bloco))
            .Should().Equal("0", "C", "E", "J", "K", "L", "M", "N", "P", "Q", "T", "U", "V", "W", "X", "Y", "9");

        AssertRegistroEcf.CatalogMatchesManifest();
    }

    [Fact]
    public void Catalogo_SoDivergeDoManifestoNosSeteRemovidosNoLeiaute11()
    {
        var manifesto = ManifestoEcf.Carregar().CodigosCanonicos.ToHashSet(StringComparer.Ordinal);
        var catalogo = new CatalogoSpedGerado().EnumerarRegistros()
            .Select(registro => registro.Codigo).ToHashSet(StringComparer.Ordinal);

        catalogo.Except(manifesto).Should().BeEquivalentTo(RegistrosRemovidosLeiaute11Tests.CodigosRemovidos);
        manifesto.Except(catalogo).Should().BeEmpty();
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
    public void CatalogoComRegistroAusente_ApontaCodigoEQuantidade()
    {
        MetadadosRegistro[] catalogo = CatalogoAtual()
            .Where(registro => registro.Codigo != "9999")
            .ToArray();

        var act = () => AssertRegistroEcf.CatalogMatchesManifest(catalogo);

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*quantidade*180*179*ausentes*9999*");
    }

    [Fact]
    public void CatalogoComRegistroExtra_ApontaCodigoEQuantidade()
    {
        var catalogo = CatalogoAtual().ToList();
        catalogo.Add(Copiar(catalogo[0], codigo: "ZZZZ"));

        var act = () => AssertRegistroEcf.CatalogMatchesManifest(catalogo);

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*quantidade*180*181*extras*ZZZZ*");
    }

    [Fact]
    public void CatalogoComCodigoDuplicado_ApontaCodigoEPosicoes()
    {
        var catalogo = CatalogoAtual();
        catalogo[1] = Copiar(catalogo[0]);

        var act = () => AssertRegistroEcf.CatalogMatchesManifest(catalogo);

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*duplicado*0000*posições 1, 2*");
    }

    [Fact]
    public void CatalogoReordenado_ApontaPrimeiraDivergencia()
    {
        var catalogo = CatalogoAtual();
        (catalogo[0], catalogo[1]) = (catalogo[1], catalogo[0]);

        var act = () => AssertRegistroEcf.CatalogMatchesManifest(catalogo);

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*ordem canônica*posição 1*esperado '0000'*encontrado '0001'*");
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
    public void CatalogoComNomeDeCampoDivergente_ApontaRegistroCampoEValores()
    {
        var catalogo = CatalogoAtual();
        int indice = Array.FindIndex(catalogo, registro => registro.Codigo == "9100");
        MetadadosRegistro original = catalogo[indice];
        MetadadosCampo campoOriginal = original.Campos[4];
        MetadadosCampo campoDivergente = Copiar(campoOriginal, nome: "CONTEUDO");
        MetadadosCampo[] campos = original.Campos.ToArray();
        campos[4] = campoDivergente;
        catalogo[indice] = Copiar(original, campos: campos);

        var act = () => AssertRegistroEcf.CatalogMatchesManifest(catalogo);

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*registro 9100, campo nº 6 CONTEÚDO*nome esperado 'CONTEÚDO', encontrado 'CONTEUDO'*");
    }

    [Fact]
    public void CatalogoComNivelDivergente_ApontaRegistroEValores()
    {
        var catalogo = CatalogoAtual();
        catalogo[0] = Copiar(catalogo[0], nivel: catalogo[0].Nivel + 1);

        var act = () => AssertRegistroEcf.CatalogMatchesManifest(catalogo);

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*0000*nível esperado '0', encontrado '1'*");
    }

    [Fact]
    public void CatalogoComBlocoDivergente_ApontaRegistroEValores()
    {
        var catalogo = CatalogoAtual();
        catalogo[0] = Copiar(catalogo[0], bloco: "Z");

        var act = () => AssertRegistroEcf.CatalogMatchesManifest(catalogo);

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*0000*bloco esperado '0', encontrado 'Z'*");
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

    private static MetadadosRegistro[] CatalogoAtual()
        => new CatalogoSpedGerado().EnumerarRegistros().ToArray();

    private static List<string> ExtrairBlocosEmOrdem(IEnumerable<string> blocos)
    {
        var resultado = new List<string>();
        foreach (string bloco in blocos)
        {
            if (resultado.Count == 0 || !string.Equals(resultado[^1], bloco, StringComparison.Ordinal))
                resultado.Add(bloco);
        }

        return resultado;
    }

    private static MetadadosRegistro Copiar(
        MetadadosRegistro original,
        string? codigo = null,
        int? nivel = null,
        string? bloco = null,
        IReadOnlyList<MetadadosCampo>? campos = null)
        => new(
            codigo ?? original.Codigo,
            nivel ?? original.Nivel,
            bloco ?? original.Bloco,
            original.TipoCSharp,
            original.Fabrica,
            campos ?? original.Campos,
            original.IntroduzidoEm,
            original.DescontinuadoEm,
            original.TokenFimArquivo);

    private static MetadadosCampo Copiar(MetadadosCampo original, string nome)
        => new(
            nome,
            original.Ordem,
            original.Tipo,
            original.Tamanho,
            original.Decimais,
            original.Obrigatorio,
            original.Formato,
            original.Definidor,
            original.Serializar,
            original.DesdeVersao,
            original.CapturaTudo,
            original.CampoArquivo);
}
