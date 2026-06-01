using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.230 - exercita a forma do <see cref="Registro1390"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 279): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1390Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1390).Assembly);

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
    public void Atributo_Declara1390_Nivel2_Bloco1()
    {
        var atributo = (RegistroSpedAttribute?)Attribute.GetCustomAttribute(
            typeof(Registro1390),
            typeof(RegistroSpedAttribute));

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1390");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1390ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("1390".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1390");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodProd"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
        meta.Campos.Should().OnlyContain(c => c.Obrigatorio);
        meta.Campos[0].Tamanho.Should().Be(2);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1390".AsSpan(), out var meta);
        var registro = (Registro1390)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());

        registro.CodProd.Should().Be(1);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveZero()
    {
        _catalogo.TentarObter("1390".AsSpan(), out var meta);
        var registro = (Registro1390)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.CodProd.Should().Be(0);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1390|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComProdutoAlternativo_PreservaTextoCanonico()
    {
        const string sped = "|1390|2|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
