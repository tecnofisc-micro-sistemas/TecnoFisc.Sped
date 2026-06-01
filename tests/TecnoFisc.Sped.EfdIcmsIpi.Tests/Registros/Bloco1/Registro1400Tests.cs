using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.232 - exercita a forma do <see cref="Registro1400"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 282): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1400Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1400).Assembly);

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
    public void Atributo_Declara1400_Nivel2_Bloco1()
    {
        var atributo = (RegistroSpedAttribute?)Attribute.GetCustomAttribute(
            typeof(Registro1400),
            typeof(RegistroSpedAttribute));

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1400");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1400ComTresCamposNaOrdem()
    {
        _catalogo.TentarObter("1400".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1400");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodItemIpm",
            "Mun",
            "Valor",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 3));
        meta.Campos.Should().OnlyContain(c => c.Obrigatorio);
        meta.Campos[0].Tamanho.Should().Be(60);
        meta.Campos[1].Tamanho.Should().Be(7);
        meta.Campos[2].Tamanho.Should().Be(0);
        meta.Campos[2].Decimais.Should().Be(2);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1400".AsSpan(), out var meta);
        var registro = (Registro1400)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "ITEM-IPM-001".AsSpan());
        meta.Campos[1].Definidor(registro, "3550308".AsSpan());
        meta.Campos[2].Definidor(registro, "12345,67".AsSpan());

        registro.CodItemIpm.Should().Be("ITEM-IPM-001");
        registro.Mun.Should().Be(3550308);
        registro.Valor.Should().Be(12345.67m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNuloOuZero()
    {
        _catalogo.TentarObter("1400".AsSpan(), out var meta);
        var registro = (Registro1400)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.CodItemIpm.Should().BeNull();
        registro.Mun.Should().Be(0);
        registro.Valor.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1400|ITEM-IPM-001|3550308|12345,67|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComOutroMunicipio_PreservaTextoCanonico()
    {
        const string sped = "|1400|PRODUTO-AGREGADO|3304557|987,65|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
