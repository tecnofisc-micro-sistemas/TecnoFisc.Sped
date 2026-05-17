using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoB;

/// <summary>
/// Sub-stage 8.026 — exercita a forma do <see cref="RegistroB030"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 48): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroB030Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroB030).Assembly);

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
    public void Atributo_DeclaraB030_Nivel2_BlocoB()
    {
        var atributo = typeof(RegistroB030).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("B030");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("B");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroB030Com11CamposNaOrdem()
    {
        _catalogo.TentarObter("B030".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("B030");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMod", "Ser", "NumDocIni", "NumDocFin", "DtDoc",
            "QtdCanc", "VlCont", "VlIsntIss", "VlBcIss", "VlIss", "CodInfObs",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 11));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("B030".AsSpan(), out var meta);
        var registro = (RegistroB030)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "3A".AsSpan());        // CodMod
        meta.Campos[1].Definidor(registro, "001".AsSpan());       // Ser
        meta.Campos[2].Definidor(registro, "1".AsSpan());         // NumDocIni
        meta.Campos[3].Definidor(registro, "100".AsSpan());       // NumDocFin
        meta.Campos[4].Definidor(registro, "15052024".AsSpan());  // DtDoc
        meta.Campos[5].Definidor(registro, "2".AsSpan());         // QtdCanc
        meta.Campos[6].Definidor(registro, "10000.00".AsSpan());  // VlCont
        meta.Campos[7].Definidor(registro, "500.00".AsSpan());    // VlIsntIss
        meta.Campos[8].Definidor(registro, "9000.00".AsSpan());   // VlBcIss
        meta.Campos[9].Definidor(registro, "450.00".AsSpan());    // VlIss
        meta.Campos[10].Definidor(registro, "OBS001".AsSpan());   // CodInfObs

        registro.CodMod.Should().Be("3A");
        registro.Ser.Should().Be("001");
        registro.NumDocIni.Should().Be(1);
        registro.NumDocFin.Should().Be(100);
        registro.DtDoc.Should().Be(new DateOnly(2024, 5, 15));
        registro.QtdCanc.Should().Be(2);
        registro.VlCont.Should().Be(10000.00m);
        registro.VlIsntIss.Should().Be(500.00m);
        registro.VlBcIss.Should().Be(9000.00m);
        registro.VlIss.Should().Be(450.00m);
        registro.CodInfObs.Should().Be("OBS001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("B030".AsSpan(), out var meta);
        var registro = (RegistroB030)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, Span<char>.Empty);   // Ser
        registro.Ser.Should().BeNull();

        meta.Campos[10].Definidor(registro, Span<char>.Empty);  // CodInfObs
        registro.CodInfObs.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|B030|3A|001|1|100|15052024|2|10000,00|500,00|9000,00|450,00|OBS001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Sem série e sem COD_INF_OBS — NFS-Simplificada sem cancelados.
        const string sped = "|B030|3A||1|100|15052024|0|5000,00|0,00|5000,00|250,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComDocumentosCancelados_PreservaTextoCanonico()
    {
        // 5 cancelados dentro da faixa 500-520.
        const string sped = "|B030|3A|S|500|520|20122023|5|75000,00|1000,00|74000,00|3700,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
