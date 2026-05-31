using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.228 - exercita a forma do <see cref="Registro1360"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 279): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1360Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1360).Assembly);

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
    public void Atributo_Declara1360_Nivel3_Bloco1()
    {
        var atributo = (RegistroSpedAttribute?)Attribute.GetCustomAttribute(
            typeof(Registro1360),
            typeof(RegistroSpedAttribute));

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1360");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1360ComDoisCamposNaOrdem()
    {
        _catalogo.TentarObter("1360".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1360");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumLacre",
            "DtAplicacao",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
        meta.Campos.Should().OnlyContain(c => c.Obrigatorio);
        meta.Campos[0].Tamanho.Should().Be(20);
        meta.Campos[1].Tamanho.Should().Be(8);
        meta.Campos[1].Formato.Should().Be("ddMMyyyy");
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1360".AsSpan(), out var meta);
        var registro = (Registro1360)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "LACRE-000000000001".AsSpan());
        meta.Campos[1].Definidor(registro, "15032024".AsSpan());

        registro.NumLacre.Should().Be("LACRE-000000000001");
        registro.DtAplicacao.Should().Be(new DateOnly(2024, 3, 15));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNuloOuDefault()
    {
        _catalogo.TentarObter("1360".AsSpan(), out var meta);
        var registro = (Registro1360)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.NumLacre.Should().BeNull();
        registro.DtAplicacao.Should().Be(default(DateOnly));
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1360|LACRE-000000000001|15032024|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComOutroLacre_PreservaTextoCanonico()
    {
        const string sped = "|1360|BOMBA-01-LACRE-002|01012025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
