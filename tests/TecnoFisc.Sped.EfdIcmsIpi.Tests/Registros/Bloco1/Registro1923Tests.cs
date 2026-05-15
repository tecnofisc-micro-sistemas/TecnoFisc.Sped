using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.244 - exercita a forma do <see cref="Registro1923"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 294): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1923Tests
{
    private const string ChaveNfeValida = "35240111222333000181550010000000011000000018";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1923).Assembly);

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
    public void Atributo_Declara1923_Nivel6_Bloco1()
    {
        var atributo = typeof(Registro1923).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1923");
        atributo.Nivel.Should().Be(6);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1923Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("1923".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1923");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodPart", "CodMod", "Ser", "Sub", "NumDoc", "DtDoc", "CodItem", "VlAjItem", "ChvDoce",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 9));
        meta.Campos.Select(c => c.Tamanho).Should().Equal([60, 2, 4, 3, 9, 8, 60, 0, 44]);
        meta.Campos.Select(c => c.Decimais).Should().Equal([0, 0, 0, 0, 0, 0, 0, 2, 0]);
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "CodPart", "CodMod", "NumDoc", "DtDoc", "VlAjItem",
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1923".AsSpan(), out var meta);
        var registro = (Registro1923)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "PART001".AsSpan());
        meta.Campos[1].Definidor(registro, "55".AsSpan());
        meta.Campos[2].Definidor(registro, "A".AsSpan());
        meta.Campos[3].Definidor(registro, "1".AsSpan());
        meta.Campos[4].Definidor(registro, "12345".AsSpan());
        meta.Campos[5].Definidor(registro, "01012023".AsSpan());
        meta.Campos[6].Definidor(registro, "ITEM001".AsSpan());
        meta.Campos[7].Definidor(registro, "1500,00".AsSpan());
        meta.Campos[8].Definidor(registro, ChaveNfeValida.AsSpan());

        registro.CodPart.Should().Be("PART001");
        registro.CodMod.Should().Be("55");
        registro.Ser.Should().Be("A");
        registro.Sub.Should().Be(1);
        registro.NumDoc.Should().Be(12345);
        registro.DtDoc.Should().Be(new DateOnly(2023, 1, 1));
        registro.CodItem.Should().Be("ITEM001");
        registro.VlAjItem.Should().Be(1500.00m);
        registro.ChvDoce.Should().Be(ChaveAcesso.Criar(ChaveNfeValida));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1923".AsSpan(), out var meta);
        var registro = (Registro1923)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, Span<char>.Empty);
        meta.Campos[3].Definidor(registro, Span<char>.Empty);
        meta.Campos[6].Definidor(registro, Span<char>.Empty);
        meta.Campos[8].Definidor(registro, Span<char>.Empty);

        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.ChvDoce.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        var sped = $"|1923|PART001|55|A|1|12345|01012023|ITEM001|1500,00|{ChaveNfeValida}|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped = "|1923|PART001|55|||12345|01012023||1500,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
