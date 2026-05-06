using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco1;

public sealed class Registro1200Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1200).Assembly);

    [Fact]
    public void Atributo_Declara1200_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1200).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1200");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1200Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("1200".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1200");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "PerApurAnt", "NatContRec",
            "VlContApur", "VlCredPisDesc", "VlContDev", "VlOutDed", "VlContExt",
            "VlMul", "VlJur",
            "DtRecol",
        ]);
        meta.Campos[0].Tamanho.Should().Be(6);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // PerApurAnt
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // NatContRec
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // VlContApur
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // VlCredPisDesc
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // VlContDev
        meta.Campos[5].Obrigatorio.Should().BeTrue();   // VlOutDed
        meta.Campos[6].Obrigatorio.Should().BeTrue();   // VlContExt
        meta.Campos[7].Obrigatorio.Should().BeFalse();  // VlMul
        meta.Campos[8].Obrigatorio.Should().BeFalse();  // VlJur
        meta.Campos[9].Tamanho.Should().Be(8);
        meta.Campos[9].Obrigatorio.Should().BeFalse();  // DtRecol
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1200".AsSpan(), out var meta);
        var registro = (Registro1200)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "012021".AsSpan());     // PerApurAnt
        meta.Campos[1].Definidor(registro, "01".AsSpan());         // NatContRec
        meta.Campos[2].Definidor(registro, "1000,00".AsSpan());    // VlContApur
        meta.Campos[3].Definidor(registro, "200,00".AsSpan());     // VlCredPisDesc
        meta.Campos[4].Definidor(registro, "800,00".AsSpan());     // VlContDev
        meta.Campos[5].Definidor(registro, "50,00".AsSpan());      // VlOutDed
        meta.Campos[6].Definidor(registro, "750,00".AsSpan());     // VlContExt
        meta.Campos[7].Definidor(registro, "30,00".AsSpan());      // VlMul
        meta.Campos[8].Definidor(registro, "10,00".AsSpan());      // VlJur
        meta.Campos[9].Definidor(registro, "15032021".AsSpan());   // DtRecol

        registro.PerApurAnt.Should().Be("012021");
        registro.NatContRec.Should().Be(CodigoContribuicaoSocialApurada.NaoCumulativaAliquotaBasica);
        registro.VlContApur.Should().Be(1000.00m);
        registro.VlCredPisDesc.Should().Be(200.00m);
        registro.VlContDev.Should().Be(800.00m);
        registro.VlOutDed.Should().Be(50.00m);
        registro.VlContExt.Should().Be(750.00m);
        registro.VlMul.Should().Be(30.00m);
        registro.VlJur.Should().Be(10.00m);
        registro.DtRecol.Should().Be(new DateOnly(2021, 3, 15));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1200".AsSpan(), out var meta);
        var registro = (Registro1200)meta!.Fabrica();

        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlMul
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlJur
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // DtRecol

        registro.VlMul.Should().BeNull();
        registro.VlJur.Should().BeNull();
        registro.DtRecol.Should().BeNull();
    }

    [Theory]
    [InlineData("01", CodigoContribuicaoSocialApurada.NaoCumulativaAliquotaBasica)]
    [InlineData("51", CodigoContribuicaoSocialApurada.CumulativaAliquotaBasica)]
    [InlineData("99", CodigoContribuicaoSocialApurada.PisPasepFolhaSalarios)]
    public void Definidor_NatContRec_AtribuiEnumCorreto(string codigo, CodigoContribuicaoSocialApurada esperado)
    {
        _catalogo.TentarObter("1200".AsSpan(), out var meta);
        var registro = (Registro1200)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, codigo.AsSpan());

        registro.NatContRec.Should().Be(esperado);
    }

    [Theory]
    [InlineData(CodigoContribuicaoSocialApurada.NaoCumulativaAliquotaBasica, "01")]
    [InlineData(CodigoContribuicaoSocialApurada.CumulativaAliquotaBasica, "51")]
    [InlineData(CodigoContribuicaoSocialApurada.PisPasepFolhaSalarios, "99")]
    public void Serializar_NatContRec_RetornaCodigoSpedCorreto(CodigoContribuicaoSocialApurada nat, string esperado)
    {
        _catalogo.TentarObter("1200".AsSpan(), out var meta);
        var registro = (Registro1200)meta!.Fabrica();
        registro.NatContRec = nat;

        meta.Campos[1].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1200|012021|01|1000,00|200,00|800,00|50,00|750,00|30,00|10,00|15032021|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemMultaJurosEData_PreservaTextoCanonico()
    {
        const string sped = "|1200|062020|51|500,00|0,00|500,00|0,00|500,00||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ContribuicaoCumulativaComDeducoes_PreservaTextoCanonico()
    {
        const string sped = "|1200|032019|52|2500,00|300,00|2200,00|100,00|2100,00|105,00|52,50|20062019|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
