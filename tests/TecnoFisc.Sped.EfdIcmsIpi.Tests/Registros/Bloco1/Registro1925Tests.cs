using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.245 - exercita a forma do <see cref="Registro1925"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 295): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1925Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1925).Assembly);

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
    public void Atributo_Declara1925_Nivel5_Bloco1()
    {
        var atributo = typeof(Registro1925).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1925");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1925Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("1925".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1925");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodInfAdic", "VlInfAdic", "DescrComplAj"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
        meta.Campos.Select(c => c.Tamanho).Should().Equal([8, 0, 0]);
        meta.Campos.Select(c => c.Decimais).Should().Equal([0, 2, 0]);
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal(["CodInfAdic", "VlInfAdic"]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1925".AsSpan(), out var meta);
        var registro = (Registro1925)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "SP000001".AsSpan());
        meta.Campos[1].Definidor(registro, "1234,56".AsSpan());
        meta.Campos[2].Definidor(registro, "Valor declaratorio".AsSpan());

        registro.CodInfAdic.Should().Be("SP000001");
        registro.VlInfAdic.Should().Be(1234.56m);
        registro.DescrComplAj.Should().Be("Valor declaratorio");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1925".AsSpan(), out var meta);
        var registro = (Registro1925)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);
        meta.Campos[2].Definidor(registro, Span<char>.Empty);

        registro.CodInfAdic.Should().BeNull();
        registro.DescrComplAj.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|1925|SP000001|1234,56|Valor declaratorio|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemDescricaoComplementar_PreservaTextoCanonico()
    {
        const string sped =
            "|1925|SP000002|500,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
