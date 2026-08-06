using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Catalogo;

public sealed class CatalogoBuilderNomeCampoTests
{
    [Fact]
    public void AliasExplicito_DirigeMetadadoParseErroESerializacaoPosicional()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(typeof(RegistroA100AliasSintetico).Assembly);
        catalogo.TentarObter("A100", out var metadados).Should().BeTrue();

        var campo = metadados!.Campos.Should().ContainSingle().Which;
        campo.Nome.Should().Be("CODIGO");

        var registro = (RegistroA100AliasSintetico)metadados.Fabrica();
        campo.Definidor(registro, "0007");
        registro.CampoCodigo.Should().Be(7);
        campo.Serializar(registro).Should().Be("7");

        var invalido = new LeitorSpedTxt(catalogo).ParseLinha("|A100|INVALIDO|");
        invalido.Sucesso.Should().BeTrue();
        invalido.Valor.Should().BeOfType<RegistroA100AliasSintetico>()
            .Which.ErrosDeFormato.Should().ContainSingle()
            .Which.Campo.Should().Be("CODIGO");
    }

    [Theory]
    [MemberData(nameof(AliasInvalidos))]
    public void AliasExplicitoInvalido_FalhaAntesDeConstruirMetadados(Type tipo, string alias)
    {
        var act = () => Construir(tipo);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*Nome*{alias.Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n")}*");
    }

    [Fact]
    public void AliasEfetivoDuplicado_FalhaAntesDeConstruirMetadados()
    {
        var act = () => Construir(typeof(RegistroAliasDuplicadoSintetico));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Nome de campo duplicado*CODIGO*");
    }

    [Fact]
    public void NomeVazioOuNulo_UsaNomeDaPropriedadeComoAntes()
    {
        var metadados = Construir(typeof(RegistroAliasVazioOuNuloSintetico));

        metadados.Campos.Select(campo => campo.Nome).Should().Equal(
            nameof(RegistroAliasVazioOuNuloSintetico.CampoVazio),
            nameof(RegistroAliasVazioOuNuloSintetico.CampoNulo));
    }

    [Fact]
    public void NomesSemAlias_DiferenciadosSomentePorCaixa_ContinuamValidos()
    {
        var metadados = Construir(typeof(RegistroNomesSemAliasPorCaixaSintetico));

        metadados.Campos.Select(campo => campo.Nome).Should().Equal("Valor", "VALOR");
    }

    [Fact]
    public void AliasesDiferenciadosSomentePorCaixa_ContinuamValidos()
    {
        var metadados = Construir(typeof(RegistroAliasesPorCaixaSintetico));

        metadados.Campos.Select(campo => campo.Nome).Should().Equal("Valor", "VALOR");
    }

    public static TheoryData<Type, string> AliasInvalidos => new()
    {
        { typeof(RegistroAliasEspacoSintetico), " " },
        { typeof(RegistroAliasTabSintetico), "\t" },
        { typeof(RegistroAliasMargemSintetico), " CODIGO " },
        { typeof(RegistroAliasPipeSintetico), "COD|IGO" },
        { typeof(RegistroAliasQuebraSintetico), "COD\nIGO" },
        { typeof(RegistroAliasDigitoInicialSintetico), "1CODIGO" },
    };

    private static MetadadosRegistro Construir(Type tipo)
        => CatalogoBuilder.BuildMetadataForType(
            tipo,
            "A999",
            1,
            "A",
            () => (RegistroSped)Activator.CreateInstance(tipo)!);

    private abstract class RegistroAliasBase : RegistroSped
    {
        public override string Codigo => "A999";
    }

    private sealed class RegistroAliasEspacoSintetico : RegistroAliasBase
    {
        [CampoSped(Ordem = 2, Nome = " ")]
        public string? Campo { get; set; }
    }

    private sealed class RegistroAliasTabSintetico : RegistroAliasBase
    {
        [CampoSped(Ordem = 2, Nome = "\t")]
        public string? Campo { get; set; }
    }

    private sealed class RegistroAliasMargemSintetico : RegistroAliasBase
    {
        [CampoSped(Ordem = 2, Nome = " CODIGO ")]
        public string? Campo { get; set; }
    }

    private sealed class RegistroAliasPipeSintetico : RegistroAliasBase
    {
        [CampoSped(Ordem = 2, Nome = "COD|IGO")]
        public string? Campo { get; set; }
    }

    private sealed class RegistroAliasQuebraSintetico : RegistroAliasBase
    {
        [CampoSped(Ordem = 2, Nome = "COD\nIGO")]
        public string? Campo { get; set; }
    }

    private sealed class RegistroAliasDigitoInicialSintetico : RegistroAliasBase
    {
        [CampoSped(Ordem = 2, Nome = "1CODIGO")]
        public string? Campo { get; set; }
    }

    private sealed class RegistroAliasDuplicadoSintetico : RegistroAliasBase
    {
        [CampoSped(Ordem = 2, Nome = "CODIGO")]
        public string? Primeiro { get; set; }

        [CampoSped(Ordem = 3, Nome = "CODIGO")]
        public string? Segundo { get; set; }
    }

    private sealed class RegistroAliasVazioOuNuloSintetico : RegistroAliasBase
    {
        [CampoSped(Ordem = 2, Nome = "")]
        public string? CampoVazio { get; set; }

        [CampoSped(Ordem = 3, Nome = null)]
        public string? CampoNulo { get; set; }
    }

    private sealed class RegistroNomesSemAliasPorCaixaSintetico : RegistroAliasBase
    {
        [CampoSped(Ordem = 2)]
        public string? Valor { get; set; }

        [CampoSped(Ordem = 3)]
        public string? VALOR { get; set; }
    }

    private sealed class RegistroAliasesPorCaixaSintetico : RegistroAliasBase
    {
        [CampoSped(Ordem = 2, Nome = "Valor")]
        public string? Primeiro { get; set; }

        [CampoSped(Ordem = 3, Nome = "VALOR")]
        public string? Segundo { get; set; }
    }
}
