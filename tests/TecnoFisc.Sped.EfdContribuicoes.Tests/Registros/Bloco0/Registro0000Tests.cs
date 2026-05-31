using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 4.001 — exercita a forma do <see cref="Registro0000"/> contra o Guia Prático
/// v1.35 (p. 66): metadados de catálogo, mapeamento de campos e invariante de round-trip
/// parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0000Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigo0000_Nivel0_Bloco0()
    {
        var atributo = typeof(Registro0000).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0000");
        atributo.Nivel.Should().Be(0);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0000ComTrezeCamposNaOrdem()
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0000");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodVer",
            "TipoEscrit",
            "IndSitEsp",
            "NumRecAnterior",
            "DtIni",
            "DtFin",
            "Nome",
            "Cnpj",
            "Uf",
            "CodMun",
            "Suframa",
            "IndNatPJ",
            "IndAtiv",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]);
    }

    [Fact]
    public void Definidor_AtribuiCodigosSpedAosTiposFortes()
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "006".AsSpan());
        meta.Campos[1].Definidor(registro, "0".AsSpan());
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[4].Definidor(registro, "01012025".AsSpan());
        meta.Campos[5].Definidor(registro, "31012025".AsSpan());
        meta.Campos[6].Definidor(registro, "EMPRESA TESTE LTDA".AsSpan());
        meta.Campos[7].Definidor(registro, "11222333000181".AsSpan());
        meta.Campos[8].Definidor(registro, "SP".AsSpan());
        meta.Campos[9].Definidor(registro, "3550308".AsSpan());
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[11].Definidor(registro, "00".AsSpan());
        meta.Campos[12].Definidor(registro, "2".AsSpan());

        registro.CodVer.Should().Be("006");
        registro.TipoEscrit.Should().Be(TipoEscrituracao.Original);
        registro.IndSitEsp.Should().BeNull();
        registro.NumRecAnterior.Should().BeNull();
        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 1, 31));
        registro.Nome.Should().Be("EMPRESA TESTE LTDA");
        registro.Cnpj.ToString().Should().Be("11222333000181");
        registro.Uf.Should().Be("SP");
        registro.CodMun.Should().Be("3550308");
        registro.Suframa.Should().BeNull();
        registro.IndNatPJ.Should().Be(IndicadorNaturezaPessoaJuridica.PessoaJuridicaEmGeral);
        registro.IndAtiv.Should().Be(IndicadorAtividade.Comercio);
    }

    [Fact]
    public void Serializar_EnumDeLarguraDois_AplicaZeroPadParaIndNatPJ()
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000)meta!.Fabrica();
        registro.IndNatPJ = IndicadorNaturezaPessoaJuridica.PessoaJuridicaEmGeral;

        meta.Campos[11].Serializar(registro).Should().Be("00");
    }

    [Theory]
    [InlineData(IndicadorNaturezaPessoaJuridica.PessoaJuridicaEmGeral, "00")]
    [InlineData(IndicadorNaturezaPessoaJuridica.SociedadeCooperativa, "01")]
    [InlineData(IndicadorNaturezaPessoaJuridica.SociedadeContaParticipacao, "05")]
    public void Serializar_IndNatPJ_RetornaCodigoSpedComDoisDigitos(
        IndicadorNaturezaPessoaJuridica natureza, string esperado)
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000)meta!.Fabrica();
        registro.IndNatPJ = natureza;

        meta.Campos[11].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public void Serializar_IndNatPJNulo_DevolveVazio()
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000)meta!.Fabrica();
        registro.IndNatPJ = null;

        meta.Campos[11].Serializar(registro).Should().BeEmpty();
    }

    [Theory]
    [InlineData(IndicadorAtividade.Industrial, "0")]
    [InlineData(IndicadorAtividade.Comercio, "2")]
    [InlineData(IndicadorAtividade.Outros, "9")]
    public void Serializar_IndAtiv_RetornaCodigoNumericoSemPadicaoExtra(
        IndicadorAtividade atividade, string esperado)
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000)meta!.Fabrica();
        registro.IndAtiv = atividade;

        meta.Campos[12].Serializar(registro).Should().Be(esperado);
    }
}
