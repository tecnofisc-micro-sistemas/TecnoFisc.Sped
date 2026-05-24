using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.170 — exercita a forma do <see cref="RegistroE311"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 227-228): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE311Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE311).Assembly);

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
    public void Atributo_DeclaraE311_Nivel4_BlocoE()
    {
        var atributo = typeof(RegistroE311).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E311");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE311Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("E311".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E311");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodAjApur", "DescrComplAj", "VlAjApur"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E311".AsSpan(), out var meta);
        var registro = (RegistroE311)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "SP209999".AsSpan());       // CodAjApur
        meta.Campos[1].Definidor(registro, "Outros débitos Difal".AsSpan()); // DescrComplAj
        meta.Campos[2].Definidor(registro, "1250,00".AsSpan());        // VlAjApur

        registro.CodAjApur.Should().Be("SP209999");
        registro.DescrComplAj.Should().Be("Outros débitos Difal");
        registro.VlAjApur.Should().Be(1250.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E311".AsSpan(), out var meta);
        var registro = (RegistroE311)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.CodAjApur.Should().BeNull();
        registro.DescrComplAj.Should().BeNull();
        registro.VlAjApur.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|E311|SP209999|Outros débitos Difal|1250,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemDescricaoComplementar_PreservaTextoCanonico()
    {
        // DescrComplAj é OC — pode ser vazio quando a legislação não exigir complemento.
        const string sped = "|E311|SP229999||375,50|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
