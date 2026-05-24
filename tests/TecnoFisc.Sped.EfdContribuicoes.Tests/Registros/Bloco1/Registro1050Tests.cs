using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco1;

public sealed class Registro1050Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1050).Assembly);

    [Fact]
    public void Atributo_Declara1050_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1050).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1050");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1050Com18CamposNaOrdem()
    {
        _catalogo.TentarObter("1050".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1050");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19]);
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtRef", "IndAjBc", "Cnpj",
            "VlAjTot",
            "VlAjCst01", "VlAjCst02", "VlAjCst03", "VlAjCst04",
            "VlAjCst05", "VlAjCst06", "VlAjCst07", "VlAjCst08",
            "VlAjCst09", "VlAjCst49", "VlAjCst99",
            "IndAprop", "NumRec", "InfoCompl",
        ]);
        meta.Campos[0].Tamanho.Should().Be(8);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // DtRef
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // IndAjBc
        meta.Campos[2].Tamanho.Should().Be(14);
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // Cnpj
        meta.Campos[16].Tamanho.Should().Be(80);
        meta.Campos[16].Obrigatorio.Should().BeFalse(); // NumRec
        meta.Campos[17].Tamanho.Should().Be(0);
        meta.Campos[17].Obrigatorio.Should().BeFalse(); // InfoCompl
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1050".AsSpan(), out var meta);
        var registro = (Registro1050)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "31012019".AsSpan());       // DtRef
        meta.Campos[1].Definidor(registro, "01".AsSpan());             // IndAjBc
        meta.Campos[2].Definidor(registro, "11222333000181".AsSpan()); // Cnpj
        meta.Campos[3].Definidor(registro, "100,00".AsSpan());         // VlAjTot
        meta.Campos[4].Definidor(registro, "50,00".AsSpan());          // VlAjCst01
        meta.Campos[5].Definidor(registro, "25,00".AsSpan());          // VlAjCst02
        meta.Campos[6].Definidor(registro, "25,00".AsSpan());          // VlAjCst03
        meta.Campos[7].Definidor(registro, "0,00".AsSpan());           // VlAjCst04
        meta.Campos[8].Definidor(registro, "0,00".AsSpan());           // VlAjCst05
        meta.Campos[9].Definidor(registro, "0,00".AsSpan());           // VlAjCst06
        meta.Campos[10].Definidor(registro, "0,00".AsSpan());          // VlAjCst07
        meta.Campos[11].Definidor(registro, "0,00".AsSpan());          // VlAjCst08
        meta.Campos[12].Definidor(registro, "0,00".AsSpan());          // VlAjCst09
        meta.Campos[13].Definidor(registro, "0,00".AsSpan());          // VlAjCst49
        meta.Campos[14].Definidor(registro, "0,00".AsSpan());          // VlAjCst99
        meta.Campos[15].Definidor(registro, "01".AsSpan());            // IndAprop
        meta.Campos[16].Definidor(registro, "REC-2019-12345".AsSpan()); // NumRec
        meta.Campos[17].Definidor(registro, "Ajuste de teste".AsSpan()); // InfoCompl

        registro.DtRef.Should().Be(new DateOnly(2019, 1, 31));
        registro.IndAjBc.Should().Be("01");
        registro.Cnpj.ToString().Should().Be("11222333000181");
        registro.VlAjTot.Should().Be(100.00m);
        registro.VlAjCst01.Should().Be(50.00m);
        registro.VlAjCst02.Should().Be(25.00m);
        registro.VlAjCst03.Should().Be(25.00m);
        registro.VlAjCst04.Should().Be(0m);
        registro.VlAjCst99.Should().Be(0m);
        registro.IndAprop.Should().Be(IndicadorApropriacaoContribuicao.PisECofins);
        registro.NumRec.Should().Be("REC-2019-12345");
        registro.InfoCompl.Should().Be("Ajuste de teste");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1050".AsSpan(), out var meta);
        var registro = (Registro1050)meta!.Fabrica();

        meta.Campos[16].Definidor(registro, ReadOnlySpan<char>.Empty); // NumRec
        meta.Campos[17].Definidor(registro, ReadOnlySpan<char>.Empty); // InfoCompl

        registro.NumRec.Should().BeNull();
        registro.InfoCompl.Should().BeNull();
    }

    [Theory]
    [InlineData("01", IndicadorApropriacaoContribuicao.PisECofins)]
    [InlineData("02", IndicadorApropriacaoContribuicao.SomentePis)]
    [InlineData("03", IndicadorApropriacaoContribuicao.SomenteCofins)]
    public void Definidor_IndAprop_AtribuiEnumCorreto(string codigo, IndicadorApropriacaoContribuicao esperado)
    {
        _catalogo.TentarObter("1050".AsSpan(), out var meta);
        var registro = (Registro1050)meta!.Fabrica();

        meta.Campos[15].Definidor(registro, codigo.AsSpan());

        registro.IndAprop.Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorApropriacaoContribuicao.PisECofins, "01")]
    [InlineData(IndicadorApropriacaoContribuicao.SomentePis, "02")]
    [InlineData(IndicadorApropriacaoContribuicao.SomenteCofins, "03")]
    public void Serializar_IndAprop_RetornaCodigoSpedCorreto(IndicadorApropriacaoContribuicao aprop, string esperado)
    {
        _catalogo.TentarObter("1050".AsSpan(), out var meta);
        var registro = (Registro1050)meta!.Fabrica();
        registro.IndAprop = aprop;

        meta.Campos[15].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1050|31012019|01|11222333000181|100,00|50,00|25,00|25,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|01|REC-2019-12345|Ajuste extra apuracao|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|1050|31012019|02|11222333000181|500,00|500,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|02|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.WriteAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
