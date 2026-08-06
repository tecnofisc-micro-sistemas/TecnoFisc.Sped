using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Catalogo;

public sealed class CatalogoBuilderTests
{
    private static readonly System.Reflection.Assembly _assembly = typeof(Registro0000Sintetico).Assembly;

    [Fact]
    public void BuildFromAssembly_DescobreTodosRegistrosMarcados()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);

        var codigos = catalogo.EnumerarRegistros().Select(m => m.Codigo).ToList();

        codigos.Should().Contain(["0000", "C001", "C100", "C170", "9999"]);
    }

    [Fact]
    public void TentarObter_PorReadOnlySpan_LocalizaSemAlocarString()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        ReadOnlySpan<char> codigo = "C100".AsSpan();

        var ok = catalogo.TentarObter(codigo, out var meta);

        ok.Should().BeTrue();
        meta!.Codigo.Should().Be("C100");
        meta.Nivel.Should().Be(2);
        meta.Bloco.Should().Be("C");
    }

    [Fact]
    public void TentarObter_QuandoCodigoDesconhecido_RetornaFalse()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);

        var ok = catalogo.TentarObter("ZZZZ".AsSpan(), out var meta);

        ok.Should().BeFalse();
        meta.Should().BeNull();
    }

    [Fact]
    public void Fabrica_ProduzInstanciaCorretaSemReflexaoNoChamador()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        catalogo.TentarObter("0000".AsSpan(), out var meta);

        var registro = meta!.Fabrica();

        registro.Should().BeOfType<Registro0000Sintetico>();
        registro.Codigo.Should().Be("0000");
    }

    [Fact]
    public void Campos_OrdenadosPelaPosicaoDeclarada()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        catalogo.TentarObter("0000".AsSpan(), out var meta);

        var ordens = meta!.Campos.Select(c => c.Ordem).ToList();

        ordens.Should().Equal([2, 3, 4, 5, 6]);
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodVer", "DtIni", "DtFin", "Nome", "Cnpj"]);
    }

    [Fact]
    public void Definidor_AtribuiValoresPrimitivosEValueObjects()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000Sintetico)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "006".AsSpan());
        meta.Campos[1].Definidor(registro, "01012025".AsSpan());
        meta.Campos[2].Definidor(registro, "31012025".AsSpan());
        meta.Campos[3].Definidor(registro, "EMPRESA TESTE".AsSpan());
        meta.Campos[4].Definidor(registro, "11222333000181".AsSpan());

        registro.CodVer.Should().Be("006");
        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 1, 31));
        registro.Nome.Should().Be("EMPRESA TESTE");
        registro.Cnpj.ToString().Should().Be("11222333000181");
    }

    [Fact]
    public void Definidor_QuandoVazioEPropriedadeReferencia_AtribuiNull()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000Sintetico)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.Nome.Should().BeNull();
    }

    [Fact]
    public void Definidor_QuandoVazioEPropriedadeValor_MantemDefault()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        catalogo.TentarObter("C100".AsSpan(), out var meta);
        var registro = (RegistroC100Sintetico)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty); // CodPart (int)
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // VlDoc (decimal)

        registro.CodPart.Should().Be(0);
        registro.VlDoc.Should().Be(0m);
    }

    [Fact]
    public void ParseLinha_EnumNumericoFechadoIndefinido_RegistraErroSemAtribuirValor()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        var leitor = new LeitorSpedTxt(catalogo);

        var resultado = leitor.ParseLinha("|E100|06|03|".AsSpan());

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroE100Sintetico>().Which;
        registro.Situacao.Should().BeNull();
        registro.Permissoes.Should().Be(PermissoesSinteticas.Leitura | PermissoesSinteticas.Escrita);
        registro.ErrosDeFormato.Should().ContainSingle()
            .Which.Campo.Should().Be(nameof(RegistroE100Sintetico.Situacao));
    }

    [Theory]
    [InlineData("01", SituacaoSintetica.Ativa)]
    [InlineData("2", SituacaoSintetica.Inativa)]
    public void ParseLinha_EnumNumericoFechadoDefinido_AtribuiValor(
        string valorSped,
        SituacaoSintetica esperado)
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        var leitor = new LeitorSpedTxt(catalogo);

        var resultado = leitor.ParseLinha($"|E100|{valorSped}||".AsSpan());

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroE100Sintetico>().Which;
        registro.Situacao.Should().Be(esperado);
        registro.Permissoes.Should().BeNull();
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Serializar_ProduzRepresentacaoSPEDPrimitivosEValueObjects()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000Sintetico)meta!.Fabrica();
        registro.CodVer = "006";
        registro.DtIni = new DateOnly(2025, 1, 1);
        registro.DtFin = new DateOnly(2025, 1, 31);
        registro.Nome = "EMPRESA TESTE";
        registro.Cnpj = Cnpj.Create("11222333000181");

        meta.Campos[0].Serializar(registro).Should().Be("006");
        meta.Campos[1].Serializar(registro).Should().Be("01012025");
        meta.Campos[2].Serializar(registro).Should().Be("31012025");
        meta.Campos[3].Serializar(registro).Should().Be("EMPRESA TESTE");
        meta.Campos[4].Serializar(registro).Should().Be("11222333000181");
    }

    [Fact]
    public void Serializar_QuandoStringNula_DevolveVazio()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000Sintetico)meta!.Fabrica();
        registro.Nome = null;

        meta.Campos[3].Serializar(registro).Should().BeEmpty();
    }

    [Fact]
    public void Serializar_AplicaCasasDecimaisDoLayout()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        catalogo.TentarObter("C100".AsSpan(), out var meta);
        var registro = (RegistroC100Sintetico)meta!.Fabrica();
        registro.VlDoc = 1500.75m;

        meta.Campos[2].Serializar(registro).Should().Be("1500,75");
    }

    [Fact]
    public void BuildFromAssembly_SegundaChamada_DevolveMesmaInstanciaDoCache()
    {
        var primeiro = CatalogoBuilder.BuildFromAssembly(_assembly);
        var segundo = CatalogoBuilder.BuildFromAssembly(_assembly);

        segundo.Should().BeSameAs(primeiro);
    }

    [Fact]
    public void BuildFromAssembly_ComAssemblyNulo_LancaArgumentNullException()
    {
        var act = () => CatalogoBuilder.BuildFromAssembly(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Metadados_QuandoRegistroSemVersao_IntroduzidoEmZero()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        catalogo.TentarObter("0000".AsSpan(), out var meta);

        meta!.IntroduzidoEm.Should().Be(0);
        meta.Campos.Should().OnlyContain(c => c.DesdeVersao == 0);
    }

    [Fact]
    public void Metadados_QuandoRegistroVersionado_PropagaIntroduzidoEm()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        catalogo.TentarObter("Z100".AsSpan(), out var meta);

        meta!.IntroduzidoEm.Should().Be(310);
    }

    [Fact]
    public void Metadados_QuandoCampoVersionado_PropagaDesdeVersao()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        catalogo.TentarObter("Z100".AsSpan(), out var meta);

        meta!.Campos.Should().HaveCount(2);
        meta.Campos[0].Nome.Should().Be("CodConf");
        meta.Campos[0].DesdeVersao.Should().Be(0);
        meta.Campos[1].Nome.Should().Be("IndComplemento");
        meta.Campos[1].DesdeVersao.Should().Be(312);
    }

    [Fact]
    public void Metadados_QuandoRegistroMultilinha_PropagaTokenFimArquivoECampoArquivo()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        catalogo.TentarObter("Y800".AsSpan(), out var meta);

        meta!.TokenFimArquivo.Should().Be("Y800FIM");
        meta.EhMultilinha.Should().BeTrue();

        var arqRtf = meta.Campos.Single(c => c.Nome == "ArqRtf");
        arqRtf.CampoArquivo.Should().BeTrue();
        meta.Campos.Where(c => c.Nome != "ArqRtf").Should().OnlyContain(c => !c.CampoArquivo);
    }

    [Fact]
    public void Metadados_QuandoRegistroUmaLinha_TokenFimArquivoNuloENaoMultilinha()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(_assembly);
        catalogo.TentarObter("0000".AsSpan(), out var meta);

        meta!.TokenFimArquivo.Should().BeNull();
        meta.EhMultilinha.Should().BeFalse();
        meta.Campos.Should().OnlyContain(c => !c.CampoArquivo);
    }
}
