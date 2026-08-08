using System.Text.Json.Nodes;

using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Enums;
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
            IsAvailable(introduzidoEm, introduzidoEm - 1)
                .Should().BeFalse($"{codigo} ainda não existia no leiaute anterior");
            IsAvailable(introduzidoEm, introduzidoEm)
                .Should().BeTrue($"{codigo} passa a existir no leiaute declarado");
        }

        IsAvailable(10, 9).Should().BeFalse();
        IsAvailable(10, 10).Should().BeTrue();
        IsAvailable(12, 11).Should().BeFalse();
        IsAvailable(12, 12).Should().BeTrue();
    }

    [Theory]
    [InlineData(9, "Y750", "|Y750|000001|DESCRICAO|VALOR|")]
    [InlineData(10, "N605", "|N605|000111||-10000,25|D|")]
    [InlineData(10, "X360", "|X360|000001|DESCRICAO DINAMICA|R$ -1.234,56|")]
    [InlineData(10, "X365", "|X365|E00001|ENTIDADE CONTROLADA|")]
    [InlineData(10, "X366", "|X366|000002|RELACAO DINAMICA|VALOR LIVRE|")]
    [InlineData(10, "X370", "|X370|E00001|03|ENTIDADE CONTROLADA|076||101|SERVICOS CONTROLADOS|1234,56|S|100,00|25,50|01|MLT|JUSTIFICATIVA|S|N|S|N|S|")]
    [InlineData(10, "X371", "|X371|2328.2.0001||25,00|D|")]
    [InlineData(10, "X375", "|X375|000003|METODO DINAMICO|VALOR SEM NORMALIZACAO|")]
    [InlineData(10, "X485", "|X485|12|ATO DECLARATORIO|11222333000181|000000000000000123|000000000000000456|000000000000000789|123/2025|06012025|01012025|31122025|")]
    [InlineData(11, "X451", "|X451|000001|REMESSA|VALOR|")]
    [InlineData(12, "Y730", "|Y730|0010|0050|31122025|PJ|00394460000141|-999,99|OBSERVACAO|")]
    public async Task Parser_RespeitaIntroducaoDoRegistroNaVersaoDeclarada(
        int introduzidoEm,
        string codigo,
        string linha)
    {
        var abaixo = await FixtureEcf.ReadAsync(introduzidoEm - 1, linha);
        var naIntroducao = await FixtureEcf.ReadAsync(introduzidoEm, linha);

        // Abaixo da versão de introdução, o registro real nunca aparece — mas o descarte por
        // vigência (achado 2 do PR 531) não é mais mudo: a linha vira RegistroNaoReconhecido,
        // então a ausência precisa ser verificada excluindo a sentinela, e a sentinela em si
        // precisa ser conferida explicitamente (não é ruído a ignorar). `is not` em pattern não
        // é suportado em árvore de expressão, daí o filtro por tipo concreto via `Any` comum.
        bool registroRealAbaixo = abaixo.Any(registro =>
            registro.Codigo == codigo && registro.GetType() != typeof(RegistroNaoReconhecido));
        registroRealAbaixo.Should().BeFalse($"{codigo} ainda não existia no leiaute anterior");
        abaixo.OfType<RegistroNaoReconhecido>().Should().ContainSingle(registro => registro.Codigo == codigo)
            .Which.Erro.Mensagem.Should().StartWith("Registro posterior à versão declarada no 0000");
        naIntroducao.Should().ContainSingle(registro => registro.Codigo == codigo);
    }

    [Theory]
    [InlineData(9, false, null)]
    [InlineData(10, true, null)]
    [InlineData(11, true, null)]
    [InlineData(12, true, "CEBAS-TESTE")]
    public async Task Parser_PreservaReusoPosicional0020_31EAtivaCampo32SomenteNoLeiaute12(
        int versao,
        bool posicao31Ativa,
        string? cebasEsperado)
    {
        var valores = new List<string> { "1", "1" };
        valores.AddRange(Enumerable.Repeat("N", 27));
        valores.Add("S");
        valores.Add("CEBAS-TESTE");
        string linha0020 = "|0020|" + string.Join('|', valores) + "|";

        var registro = (await FixtureEcf.ReadAsync(versao, linha0020)).OfType<Registro0020>().Single();

        registro.IndicadorPosicao31.Should().Be(
            posicao31Ativa ? IndicadorSimNao.Sim : IndicadorSimNao.Nao);
        registro.Cebas.Should().Be(cebasEsperado);
    }

    [Fact]
    public void MetadadoOmitido_PermaneceCompativelComLeiaute8()
    {
        var registro = ManifestoEcf.Carregar().Obter("0000");

        registro.IntroducedIn.Should().Be(0);
        registro.Fields.Should().OnlyContain(campo => campo.SinceVersion == 0);
        IsAvailable(registro.IntroducedIn, 8).Should().BeTrue();
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
    public void CatalogoAtual_ReconheceRegistrosRemovidosNoLeiaute11SemModelarCampos()
    {
        var catalogo = new CatalogoSpedGerado();

        foreach (var codigo in RegistrosRemovidosLeiaute11Tests.CodigosRemovidos)
        {
            catalogo.TentarObter(codigo, out var metadados).Should().BeTrue();
            metadados!.DescontinuadoEm.Should().Be((int)LayoutEcf.V011);
            metadados.Campos.Should().BeEmpty();
        }
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

    private static bool IsAvailable(int introduzidoEm, int versao)
        => introduzidoEm == 0 || introduzidoEm <= versao;

    private static (string Manifesto, string Schema) LerArtefatosCopiados()
    {
        string diretorio = Path.Combine(AppContext.BaseDirectory, "Manifesto");
        return (
            File.ReadAllText(Path.Combine(diretorio, "layout-12-manifest.json")),
            File.ReadAllText(Path.Combine(diretorio, "layout-12-manifest.schema.json")));
    }
}
