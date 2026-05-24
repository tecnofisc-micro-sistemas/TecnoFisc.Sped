using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.147 — exercita a forma do <see cref="RegistroD600"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (pp. 197-198): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD600Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD600).Assembly);

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
    public void Atributo_DeclaraD600_Nivel2_BlocoD()
    {
        var atributo = typeof(RegistroD600).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D600");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD600Com17CamposNaOrdem()
    {
        _catalogo.TentarObter("D600".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D600");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMod", "CodMun", "Ser", "Sub", "CodCons",
            "QtdCons", "DtDoc", "VlDoc", "VlDesc", "VlServ",
            "VlServNt", "VlTerc", "VlDa", "VlBcIcms", "VlIcms",
            "VlPis", "VlCofins",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([
            2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18,
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D600".AsSpan(), out var meta);
        var registro = (RegistroD600)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "21".AsSpan());           // CodMod
        meta.Campos[1].Definidor(registro, "1234567".AsSpan());      // CodMun
        meta.Campos[2].Definidor(registro, "S01".AsSpan());          // Ser
        meta.Campos[3].Definidor(registro, "1".AsSpan());            // Sub
        meta.Campos[4].Definidor(registro, "3".AsSpan());            // CodCons
        meta.Campos[5].Definidor(registro, "10".AsSpan());           // QtdCons
        meta.Campos[6].Definidor(registro, "01012023".AsSpan());     // DtDoc
        meta.Campos[7].Definidor(registro, "50000,00".AsSpan());     // VlDoc
        meta.Campos[8].Definidor(registro, "500,00".AsSpan());       // VlDesc
        meta.Campos[9].Definidor(registro, "45000,00".AsSpan());     // VlServ
        meta.Campos[10].Definidor(registro, "2000,00".AsSpan());     // VlServNt
        meta.Campos[11].Definidor(registro, "1000,00".AsSpan());     // VlTerc
        meta.Campos[12].Definidor(registro, "700,00".AsSpan());      // VlDa
        meta.Campos[13].Definidor(registro, "40000,00".AsSpan());    // VlBcIcms
        meta.Campos[14].Definidor(registro, "4800,00".AsSpan());     // VlIcms
        meta.Campos[15].Definidor(registro, "100,00".AsSpan());      // VlPis
        meta.Campos[16].Definidor(registro, "200,00".AsSpan());      // VlCofins

        registro.CodMod.Should().Be("21");
        registro.CodMun.Should().Be(1234567);
        registro.Ser.Should().Be("S01");
        registro.Sub.Should().Be(1);
        registro.CodCons.Should().Be(3);
        registro.QtdCons.Should().Be(10);
        registro.DtDoc.Should().Be(new DateOnly(2023, 1, 1));
        registro.VlDoc.Should().Be(50000.00m);
        registro.VlDesc.Should().Be(500.00m);
        registro.VlServ.Should().Be(45000.00m);
        registro.VlServNt.Should().Be(2000.00m);
        registro.VlTerc.Should().Be(1000.00m);
        registro.VlDa.Should().Be(700.00m);
        registro.VlBcIcms.Should().Be(40000.00m);
        registro.VlIcms.Should().Be(4800.00m);
        registro.VlPis.Should().Be(100.00m);
        registro.VlCofins.Should().Be(200.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D600".AsSpan(), out var meta);
        var registro = (RegistroD600)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);   // CodMod
        meta.Campos[1].Definidor(registro, Span<char>.Empty);   // CodMun
        meta.Campos[2].Definidor(registro, Span<char>.Empty);   // Ser
        meta.Campos[3].Definidor(registro, Span<char>.Empty);   // Sub
        meta.Campos[4].Definidor(registro, Span<char>.Empty);   // CodCons
        meta.Campos[5].Definidor(registro, Span<char>.Empty);   // QtdCons
        meta.Campos[8].Definidor(registro, Span<char>.Empty);   // VlDesc
        meta.Campos[10].Definidor(registro, Span<char>.Empty);  // VlServNt
        meta.Campos[11].Definidor(registro, Span<char>.Empty);  // VlTerc
        meta.Campos[12].Definidor(registro, Span<char>.Empty);  // VlDa
        meta.Campos[15].Definidor(registro, Span<char>.Empty);  // VlPis
        meta.Campos[16].Definidor(registro, Span<char>.Empty);  // VlCofins

        registro.CodMod.Should().BeNull();
        registro.CodMun.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.CodCons.Should().BeNull();
        registro.QtdCons.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.VlServNt.Should().BeNull();
        registro.VlTerc.Should().BeNull();
        registro.VlDa.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // NF comunicação (21), município IBGE 1234567, série S01, subsérie 1, classe 3, 10 docs, 01/01/2023.
        const string sped =
            "|D600|21|1234567|S01|1|3|10|01012023|50000,00|500,00|45000,00|2000,00|1000,00|700,00|40000,00|4800,00|100,00|200,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Apenas campos obrigatórios; Sub, descontos, terceiros, PIS/COFINS vazios.
        const string sped =
            "|D600|22|9876543|T02||5|25|15032023|30000,00||25000,00||||20000,00|2400,00|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
