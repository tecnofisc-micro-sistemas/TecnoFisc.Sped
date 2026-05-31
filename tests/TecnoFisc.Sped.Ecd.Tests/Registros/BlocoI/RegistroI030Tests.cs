using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.024 — exercita a forma do <see cref="RegistroI030"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 113–117): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI030Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

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

    [Fact]
    public void Atributo_DeclaraI030_Nivel3_BlocoI()
    {
        var atributo = typeof(RegistroI030).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I030");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI030ComOnzeCamposNaOrdem()
    {
        _catalogo.TentarObter("I030".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I030");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["DnrcAbert", "NumOrd", "NatLivr", "QtdLin", "Nome", "Nire", "Cnpj", "DtArq", "DtArqConv", "DescMun", "DtExSocial"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I030".AsSpan(), out var meta);
        var registro = (RegistroI030)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "TERMO DE ABERTURA".AsSpan());
        meta.Campos[1].Definidor(registro, "1".AsSpan());
        meta.Campos[2].Definidor(registro, "Balancete".AsSpan());
        meta.Campos[3].Definidor(registro, "500".AsSpan());
        meta.Campos[4].Definidor(registro, "EMPRESA TESTE".AsSpan());
        meta.Campos[5].Definidor(registro, "31123456789".AsSpan());
        meta.Campos[6].Definidor(registro, "11222333000181".AsSpan());
        meta.Campos[7].Definidor(registro, "01012015".AsSpan());
        meta.Campos[8].Definidor(registro, "15012015".AsSpan());
        meta.Campos[9].Definidor(registro, "BELO HORIZONTE".AsSpan());
        meta.Campos[10].Definidor(registro, "31122023".AsSpan());

        registro.DnrcAbert.Should().Be("TERMO DE ABERTURA");
        registro.NumOrd.Should().Be(1);
        registro.NatLivr.Should().Be("Balancete");
        registro.QtdLin.Should().Be(500);
        registro.Nome.Should().Be("EMPRESA TESTE");
        registro.Nire.Should().Be("31123456789");
        registro.Cnpj.Should().Be(Cnpj.Create("11222333000181"));
        registro.DtArq.Should().Be(new DateOnly(2015, 1, 1));
        registro.DtArqConv.Should().Be(new DateOnly(2015, 1, 15));
        registro.DescMun.Should().Be("BELO HORIZONTE");
        registro.DtExSocial.Should().Be(new DateOnly(2023, 12, 31));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I030".AsSpan(), out var meta);
        var registro = (RegistroI030)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.Nire.Should().BeNull();
        registro.DtArq.Should().BeNull();
        registro.DtArqConv.Should().BeNull();
        registro.DescMun.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|I030|TERMO DE ABERTURA|1|Balancete|500|EMPRESA TESTE|31123456789|11222333000181|01012015|15012015|BELO HORIZONTE|31122023|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped = "|I030|TERMO DE ABERTURA|1|Diário|1000|EMPRESA TESTE||11222333000181|||SAO PAULO|31122023|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ExemploDoManual_PreservaTextoCanonico()
    {
        const string sped = "|I030|TERMO DE ABERTURA|1|Balancete|500|EMPRESA TESTE|31123456789|11222333000181|01012015||BELO HORIZONTE|31122023|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
