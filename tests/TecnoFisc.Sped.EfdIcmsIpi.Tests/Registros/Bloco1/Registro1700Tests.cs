using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.236 - exercita a forma do <see cref="Registro1700"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (pp. 286-287): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1700Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1700).Assembly);

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
    public void Atributo_Declara1700_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1700).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1700");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1700Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("1700".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1700");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodDisp", "CodMod", "Ser", "Sub", "NumDocIni", "NumDocFin", "NumAut"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 7));
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "CodDisp", "CodMod", "NumDocIni", "NumDocFin", "NumAut"
        ]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[4].Tamanho.Should().Be(12);
        meta.Campos[6].Tamanho.Should().Be(60);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1700".AsSpan(), out var meta);
        var registro = (Registro1700)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());
        meta.Campos[1].Definidor(registro, "55".AsSpan());
        meta.Campos[2].Definidor(registro, "A1".AsSpan());
        meta.Campos[3].Definidor(registro, "001".AsSpan());
        meta.Campos[4].Definidor(registro, "35".AsSpan());
        meta.Campos[5].Definidor(registro, "55".AsSpan());
        meta.Campos[6].Definidor(registro, "98765432101234567890".AsSpan());

        registro.CodDisp.Should().Be(CodigoDispositivoAutorizado.FsDa);
        registro.CodMod.Should().Be("55");
        registro.Ser.Should().Be("A1");
        registro.Sub.Should().Be("001");
        registro.NumDocIni.Should().Be(35);
        registro.NumDocFin.Should().Be(55);
        registro.NumAut.Should().Be("98765432101234567890");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1700".AsSpan(), out var meta);
        var registro = (Registro1700)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodDisp.Should().BeNull();
        registro.CodMod.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.NumDocIni.Should().Be(0);
        registro.NumDocFin.Should().Be(0);
        registro.NumAut.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1700|01|55|A1|001|35|55|98765432101234567890|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposCondicionaisVazios_PreservaTextoCanonico()
    {
        const string sped = "|1700|04|01|||100|200|1234567890|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("00", CodigoDispositivoAutorizado.FormularioSegurancaImpressorAutonomo)]
    [InlineData("01", CodigoDispositivoAutorizado.FsDa)]
    [InlineData("02", CodigoDispositivoAutorizado.FormularioSegurancaNfe)]
    [InlineData("03", CodigoDispositivoAutorizado.FormularioContinuo)]
    [InlineData("04", CodigoDispositivoAutorizado.Blocos)]
    [InlineData("05", CodigoDispositivoAutorizado.JogosSoltos)]
    public void CodDisp_Definidor_AtribuiValorCorreto(string valor, CodigoDispositivoAutorizado? esperado)
    {
        _catalogo.TentarObter("1700".AsSpan(), out var meta);
        var registro = (Registro1700)meta!.Fabrica();
        var campo = meta.Campos.First(c => c.Nome == "CodDisp");

        campo.Definidor(registro, valor.AsSpan());

        registro.CodDisp.Should().Be(esperado);
    }
}
