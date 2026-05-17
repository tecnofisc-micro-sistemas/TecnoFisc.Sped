using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.238 - exercita a forma do <see cref="Registro1800"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 288): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1800Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1800).Assembly);

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
    public void Atributo_Declara1800_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1800).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1800");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1800Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("1800".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1800");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "VlCarga", "VlPass", "VlFat", "IndRat", "VlIcmsAnt",
            "VlBcIcms", "VlIcmsApur", "VlBcIcmsApur", "VlDif"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 9));
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "VlCarga", "VlPass", "VlFat", "IndRat", "VlIcmsAnt",
            "VlBcIcms", "VlIcmsApur", "VlBcIcmsApur", "VlDif"
        ]);
        meta.Campos.Select(c => c.Decimais).Should().Equal([2, 2, 2, 6, 2, 2, 2, 2, 2]);
        meta.Campos.Select(c => c.Tamanho).Should().Equal([0, 0, 0, 8, 0, 0, 0, 0, 0]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1800".AsSpan(), out var meta);
        var registro = (Registro1800)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "120000,00".AsSpan());
        meta.Campos[1].Definidor(registro, "80000,00".AsSpan());
        meta.Campos[2].Definidor(registro, "200000,00".AsSpan());
        meta.Campos[3].Definidor(registro, "0,600000".AsSpan());
        meta.Campos[4].Definidor(registro, "30000,00".AsSpan());
        meta.Campos[5].Definidor(registro, "180000,00".AsSpan());
        meta.Campos[6].Definidor(registro, "18000,00".AsSpan());
        meta.Campos[7].Definidor(registro, "108000,00".AsSpan());
        meta.Campos[8].Definidor(registro, "12000,00".AsSpan());

        registro.VlCarga.Should().Be(120000.00m);
        registro.VlPass.Should().Be(80000.00m);
        registro.VlFat.Should().Be(200000.00m);
        registro.IndRat.Should().Be(0.600000m);
        registro.VlIcmsAnt.Should().Be(30000.00m);
        registro.VlBcIcms.Should().Be(180000.00m);
        registro.VlIcmsApur.Should().Be(18000.00m);
        registro.VlBcIcmsApur.Should().Be(108000.00m);
        registro.VlDif.Should().Be(12000.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1800".AsSpan(), out var meta);
        var registro = (Registro1800)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.VlCarga.Should().Be(0m);
        registro.VlPass.Should().Be(0m);
        registro.VlFat.Should().Be(0m);
        registro.IndRat.Should().Be(0m);
        registro.VlIcmsAnt.Should().Be(0m);
        registro.VlBcIcms.Should().Be(0m);
        registro.VlIcmsApur.Should().Be(0m);
        registro.VlBcIcmsApur.Should().Be(0m);
        registro.VlDif.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|1800|120000,00|80000,00|200000,00|0,600000|30000,00|180000,00|18000,00|108000,00|12000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComRateioIntegral_PreservaTextoCanonico()
    {
        const string sped =
            "|1800|50000,00|0,00|50000,00|1,000000|7500,00|50000,00|7500,00|50000,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
