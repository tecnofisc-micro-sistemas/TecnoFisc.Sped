using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.070 — exercita a forma do <see cref="RegistroC197"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 106): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC197Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC197).Assembly);

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
    public void Atributo_DeclaraC197_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC197).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C197");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC197Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("C197".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C197");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodAj", "DescrComplAj", "CodItem", "VlBcIcms", "AliqIcms", "VlIcms", "VlOutros"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C197".AsSpan(), out var meta);
        var registro = (RegistroC197)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "RS10000001".AsSpan());     // CodAj
        meta.Campos[1].Definidor(registro, "Ajuste ICMS-ST".AsSpan()); // DescrComplAj
        meta.Campos[2].Definidor(registro, "PROD001".AsSpan());         // CodItem
        meta.Campos[3].Definidor(registro, "1000,00".AsSpan());         // VlBcIcms
        meta.Campos[4].Definidor(registro, "12,00".AsSpan());           // AliqIcms
        meta.Campos[5].Definidor(registro, "120,00".AsSpan());          // VlIcms
        meta.Campos[6].Definidor(registro, "0,00".AsSpan());            // VlOutros

        registro.CodAj.Should().Be("RS10000001");
        registro.DescrComplAj.Should().Be("Ajuste ICMS-ST");
        registro.CodItem.Should().Be("PROD001");
        registro.VlBcIcms.Should().Be(1000m);
        registro.AliqIcms.Should().Be(12m);
        registro.VlIcms.Should().Be(120m);
        registro.VlOutros.Should().Be(0m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C197".AsSpan(), out var meta);
        var registro = (RegistroC197)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodAj.Should().BeNull();
        registro.DescrComplAj.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlOutros.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C197|RS10000001|Ajuste ICMS-ST|PROD001|1000,00|12,00|120,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SomenteObrigatorios_PreservaTextoCanonico()
    {
        // Apenas COD_AJ é obrigatório; demais campos OC podem estar vazios.
        const string sped =
            "|C197|RS10000001|||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
