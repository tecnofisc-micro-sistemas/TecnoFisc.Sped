using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoB;

/// <summary>
/// Sub-stage 8.029 — exercita a forma do <see cref="RegistroB420"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.2.2 (p. 55-56): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroB420Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroB420).Assembly);

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
    public void Atributo_DeclaraB420_Nivel2_BlocoB()
    {
        var atributo = typeof(RegistroB420).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("B420");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("B");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroB420Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("B420".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("B420");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "VlCont", "VlBcIss", "AliqIss", "VlIsntIss", "VlIss", "CodServ",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 6));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("B420".AsSpan(), out var meta);
        var registro = (RegistroB420)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "15000,00".AsSpan()); // VlCont
        meta.Campos[1].Definidor(registro, "12000,00".AsSpan()); // VlBcIss
        meta.Campos[2].Definidor(registro, "5,00".AsSpan());     // AliqIss
        meta.Campos[3].Definidor(registro, "3000,00".AsSpan());  // VlIsntIss
        meta.Campos[4].Definidor(registro, "600,00".AsSpan());   // VlIss
        meta.Campos[5].Definidor(registro, "0107".AsSpan());     // CodServ

        registro.VlCont.Should().Be(15000.00m);
        registro.VlBcIss.Should().Be(12000.00m);
        registro.AliqIss.Should().Be(5.00m);
        registro.VlIsntIss.Should().Be(3000.00m);
        registro.VlIss.Should().Be(600.00m);
        registro.CodServ.Should().Be("0107");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("B420".AsSpan(), out var meta);
        var registro = (RegistroB420)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, Span<char>.Empty); // CodServ
        registro.CodServ.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|B420|15000,00|12000,00|5,00|3000,00|600,00|0107|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AliquotaMinima_PreservaTextoCanonico()
    {
        // Alíquota ISS mínima (2%) com item de serviço distinto.
        const string sped = "|B420|8000,00|8000,00|2,00|0,00|160,00|1501|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_TodasOperacoesIsentas_PreservaTextoCanonico()
    {
        // VL_ISNT_ISS igual a VL_CONT — operações todas isentas, ISS zero.
        const string sped = "|B420|5000,00|0,00|5,00|5000,00|0,00|0801|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
