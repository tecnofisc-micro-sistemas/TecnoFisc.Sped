using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoB;

/// <summary>
/// Sub-stage 8.027 — exercita a forma do <see cref="RegistroB035"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 49-50): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroB035Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroB035).Assembly);

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
    public void Atributo_DeclaraB035_Nivel3_BlocoB()
    {
        var atributo = typeof(RegistroB035).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("B035");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("B");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroB035Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("B035".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("B035");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "VlContP", "VlBcIssP", "AliqIss", "VlIssP", "VlIsntIssP", "CodServ",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 6));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("B035".AsSpan(), out var meta);
        var registro = (RegistroB035)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "8000.00".AsSpan());   // VlContP
        meta.Campos[1].Definidor(registro, "7500.00".AsSpan());   // VlBcIssP
        meta.Campos[2].Definidor(registro, "5.00".AsSpan());      // AliqIss
        meta.Campos[3].Definidor(registro, "375.00".AsSpan());    // VlIssP
        meta.Campos[4].Definidor(registro, "500.00".AsSpan());    // VlIsntIssP
        meta.Campos[5].Definidor(registro, "0107".AsSpan());      // CodServ

        registro.VlContP.Should().Be(8000.00m);
        registro.VlBcIssP.Should().Be(7500.00m);
        registro.AliqIss.Should().Be(5.00m);
        registro.VlIssP.Should().Be(375.00m);
        registro.VlIsntIssP.Should().Be(500.00m);
        registro.CodServ.Should().Be("0107");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("B035".AsSpan(), out var meta);
        var registro = (RegistroB035)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, Span<char>.Empty);   // CodServ
        registro.CodServ.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|B035|8000,00|7500,00|5,00|375,00|500,00|0107|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemIsencao_PreservaTextoCanonico()
    {
        // Sem parcela isenta — ISS incide sobre toda a base.
        const string sped = "|B035|10000,00|10000,00|3,00|300,00|0,00|0401|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AliquotaMinima_PreservaTextoCanonico()
    {
        // Alíquota ISS mínima (2%) com combinação de item de serviço distinto.
        const string sped = "|B035|2000,00|2000,00|2,00|40,00|0,00|1701|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
