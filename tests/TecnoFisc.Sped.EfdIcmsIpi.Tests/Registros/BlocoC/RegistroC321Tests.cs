using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.074 — exercita a forma do <see cref="RegistroC321"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 109): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC321Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC321).Assembly);

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
    public void Atributo_DeclaraC321_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC321).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C321");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC321Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("C321".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C321");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CodItem", "Qtd", "Unid", "VlItem", "VlDesc", "VlBcIcms", "VlIcms", "VlPis", "VlCofins"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C321".AsSpan(), out var meta);
        var registro = (RegistroC321)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "PROD001".AsSpan());    // CodItem
        meta.Campos[1].Definidor(registro, "10,000".AsSpan());     // Qtd
        meta.Campos[2].Definidor(registro, "UN".AsSpan());         // Unid
        meta.Campos[3].Definidor(registro, "500,00".AsSpan());     // VlItem
        meta.Campos[4].Definidor(registro, "10,00".AsSpan());      // VlDesc
        meta.Campos[5].Definidor(registro, "400,00".AsSpan());     // VlBcIcms
        meta.Campos[6].Definidor(registro, "48,00".AsSpan());      // VlIcms
        meta.Campos[7].Definidor(registro, "5,00".AsSpan());       // VlPis
        meta.Campos[8].Definidor(registro, "10,00".AsSpan());      // VlCofins

        registro.CodItem.Should().Be("PROD001");
        registro.Qtd.Should().Be(10.000m);
        registro.Unid.Should().Be("UN");
        registro.VlItem.Should().Be(500.00m);
        registro.VlDesc.Should().Be(10.00m);
        registro.VlBcIcms.Should().Be(400.00m);
        registro.VlIcms.Should().Be(48.00m);
        registro.VlPis.Should().Be(5.00m);
        registro.VlCofins.Should().Be(10.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C321".AsSpan(), out var meta);
        var registro = (RegistroC321)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodItem.Should().BeNull();
        registro.Qtd.Should().BeNull();
        registro.Unid.Should().BeNull();
        registro.VlItem.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C321|PROD001|10,000|UN|500,00|10,00|400,00|48,00|5,00|10,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // VL_DESC, VL_BC_ICMS, VL_ICMS, VL_PIS e VL_COFINS são OC — podem ser vazios.
        const string sped =
            "|C321|PROD002|5,000|KG|250,00||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
