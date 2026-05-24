using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoG;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoG;

/// <summary>
/// Sub-stage 8.185 - exercita a forma do <see cref="RegistroG140"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 243-244): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class RegistroG140Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroG140).Assembly);

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
    public void Atributo_DeclaraG140_Nivel5_BlocoG()
    {
        var atributo = typeof(RegistroG140).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("G140");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("G");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroG140ComOitoCamposNaOrdem()
    {
        _catalogo.TentarObter("G140".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("G140");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumItem",
            "CodItem",
            "Qtde",
            "Unid",
            "VlIcmsOpAplicado",
            "VlIcmsStAplicado",
            "VlIcmsFrtAplicado",
            "VlIcmsDifAplicado",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("G140".AsSpan(), out var meta);
        var registro = (RegistroG140)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "001".AsSpan());
        meta.Campos[1].Definidor(registro, "ITEM-CIAP-001".AsSpan());
        meta.Campos[2].Definidor(registro, "12,34567".AsSpan());
        meta.Campos[3].Definidor(registro, "UN".AsSpan());
        meta.Campos[4].Definidor(registro, "1200,50".AsSpan());
        meta.Campos[5].Definidor(registro, "100,25".AsSpan());
        meta.Campos[6].Definidor(registro, "35,75".AsSpan());
        meta.Campos[7].Definidor(registro, "10,00".AsSpan());

        registro.NumItem.Should().Be(1);
        registro.CodItem.Should().Be("ITEM-CIAP-001");
        registro.Qtde.Should().Be(12.34567m);
        registro.Unid.Should().Be("UN");
        registro.VlIcmsOpAplicado.Should().Be(1200.50m);
        registro.VlIcmsStAplicado.Should().Be(100.25m);
        registro.VlIcmsFrtAplicado.Should().Be(35.75m);
        registro.VlIcmsDifAplicado.Should().Be(10.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("G140".AsSpan(), out var meta);
        var registro = (RegistroG140)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.NumItem.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.Qtde.Should().BeNull();
        registro.Unid.Should().BeNull();
        registro.VlIcmsOpAplicado.Should().BeNull();
        registro.VlIcmsStAplicado.Should().BeNull();
        registro.VlIcmsFrtAplicado.Should().BeNull();
        registro.VlIcmsDifAplicado.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|G140|1|ITEM-CIAP-001|12,34567|UN|1200,50|100,25|35,75|10,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresZerados_PreservaTextoCanonico()
    {
        const string sped = "|G140|2|ITEM-CIAP-002|1,00000|PC|0,00|0,00|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
