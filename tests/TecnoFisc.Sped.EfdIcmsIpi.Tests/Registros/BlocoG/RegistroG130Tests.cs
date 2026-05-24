using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoG;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoG;

/// <summary>
/// Sub-stage 8.184 - exercita a forma do <see cref="RegistroG130"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 242-243): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class RegistroG130Tests
{
    private const string ChaveNfeValida = "35240111222333000181550010000000011000000018";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroG130).Assembly);

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
    public void Atributo_DeclaraG130_Nivel4_BlocoG()
    {
        var atributo = typeof(RegistroG130).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("G130");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("G");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroG130ComOitoCamposNaOrdem()
    {
        _catalogo.TentarObter("G130".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("G130");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndEmit",
            "CodPart",
            "CodMod",
            "Serie",
            "NumDoc",
            "ChvNfeCte",
            "DtDoc",
            "NumDa",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("G130".AsSpan(), out var meta);
        var registro = (RegistroG130)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());
        meta.Campos[1].Definidor(registro, "PART001".AsSpan());
        meta.Campos[2].Definidor(registro, "55".AsSpan());
        meta.Campos[3].Definidor(registro, "001".AsSpan());
        meta.Campos[4].Definidor(registro, "000000123".AsSpan());
        meta.Campos[5].Definidor(registro, ChaveNfeValida.AsSpan());
        meta.Campos[6].Definidor(registro, "15012025".AsSpan());
        meta.Campos[7].Definidor(registro, "DA-123".AsSpan());

        registro.IndEmit.Should().Be(IndicadorEmissorDocumento.Terceiros);
        registro.CodPart.Should().Be("PART001");
        registro.CodMod.Should().Be(ModeloDocumento.Create("55"));
        registro.Serie.Should().Be("001");
        registro.NumDoc.Should().Be(123);
        registro.ChvNfeCte.Should().Be(ChaveAcesso.Create(ChaveNfeValida));
        registro.DtDoc.Should().Be(new DateOnly(2025, 1, 15));
        registro.NumDa.Should().Be("DA-123");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("G130".AsSpan(), out var meta);
        var registro = (RegistroG130)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, Span<char>.Empty);
        meta.Campos[5].Definidor(registro, Span<char>.Empty);
        meta.Campos[7].Definidor(registro, Span<char>.Empty);

        registro.Serie.Should().BeNull();
        registro.ChvNfeCte.Should().BeNull();
        registro.NumDa.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorEmissorDocumento.EmissaoPropria)]
    [InlineData("1", IndicadorEmissorDocumento.Terceiros)]
    public void Definidor_IndEmit_MapeiaCodigos(string valor, IndicadorEmissorDocumento esperado)
    {
        _catalogo.TentarObter("G130".AsSpan(), out var meta);
        var registro = (RegistroG130)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, valor.AsSpan());

        registro.IndEmit.Should().Be(esperado);
    }

    [Theory]
    [InlineData("01")]
    [InlineData("1B")]
    [InlineData("04")]
    [InlineData("07")]
    [InlineData("08")]
    [InlineData("8B")]
    [InlineData("09")]
    [InlineData("10")]
    [InlineData("26")]
    [InlineData("27")]
    [InlineData("55")]
    [InlineData("57")]
    public void Definidor_CodMod_MapeiaModelosValidosDoRegistro(string valor)
    {
        _catalogo.TentarObter("G130".AsSpan(), out var meta);
        var registro = (RegistroG130)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, valor.AsSpan());

        registro.CodMod.Should().Be(ModeloDocumento.Create(valor));
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        var sped = $"|G130|1|PART001|55|001|123|{ChaveNfeValida}|15012025|DA-123|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped = "|G130|0|PART002|01||456||31012025||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
