using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.227 - exercita a forma do <see cref="Registro1350"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 278-279): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1350Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1350).Assembly);

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

    [Fact]
    public void Atributo_Declara1350_Nivel2_Bloco1()
    {
        var atributo = (RegistroSpedAttribute?)Attribute.GetCustomAttribute(
            typeof(Registro1350),
            typeof(RegistroSpedAttribute));

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1350");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1350ComQuatroCamposNaOrdem()
    {
        _catalogo.TentarObter("1350".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1350");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "Serie",
            "Fabricante",
            "Modelo",
            "TipoMedicao",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 4));
        meta.Campos.Should().OnlyContain(c => c.Obrigatorio);
        meta.Campos[0].Tamanho.Should().Be(0);
        meta.Campos[1].Tamanho.Should().Be(60);
        meta.Campos[2].Tamanho.Should().Be(0);
        meta.Campos[3].Tamanho.Should().Be(1);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1350".AsSpan(), out var meta);
        var registro = (Registro1350)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "BBA123456".AsSpan());
        meta.Campos[1].Definidor(registro, "FABRICANTE TESTE".AsSpan());
        meta.Campos[2].Definidor(registro, "MODELO X200".AsSpan());
        meta.Campos[3].Definidor(registro, "1".AsSpan());

        registro.Serie.Should().Be("BBA123456");
        registro.Fabricante.Should().Be("FABRICANTE TESTE");
        registro.Modelo.Should().Be("MODELO X200");
        registro.TipoMedicao.Should().Be(TipoMedicaoBomba.Digital);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNuloOuDefault()
    {
        _catalogo.TentarObter("1350".AsSpan(), out var meta);
        var registro = (Registro1350)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.Serie.Should().BeNull();
        registro.Fabricante.Should().BeNull();
        registro.Modelo.Should().BeNull();
        registro.TipoMedicao.Should().Be(default(TipoMedicaoBomba));
    }

    [Theory]
    [InlineData("0", TipoMedicaoBomba.Analogico)]
    [InlineData("1", TipoMedicaoBomba.Digital)]
    public void Definidor_TipoMedicao_MapeiaCodigos(string input, TipoMedicaoBomba esperado)
    {
        _catalogo.TentarObter("1350".AsSpan(), out var meta);
        var registro = (Registro1350)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, input.AsSpan());

        registro.TipoMedicao.Should().Be(esperado);
    }

    [Theory]
    [InlineData(TipoMedicaoBomba.Analogico, "0")]
    [InlineData(TipoMedicaoBomba.Digital, "1")]
    public void Serializar_TipoMedicao_RetornaCodigo(TipoMedicaoBomba tipoMedicao, string esperado)
    {
        _catalogo.TentarObter("1350".AsSpan(), out var meta);
        var registro = (Registro1350)meta!.Fabrica();
        registro.TipoMedicao = tipoMedicao;

        meta.Campos[3].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1350|BBA123456|FABRICANTE TESTE|MODELO X200|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComMedicaoAnalogica_PreservaTextoCanonico()
    {
        const string sped = "|1350|ANLG-001|BOMBAS FISCAIS LTDA|A100|0|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
