using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoP;

public sealed class RegistroP100Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroP100).Assembly);

    [Fact]
    public void Atributo_DeclaraP100_Nivel3_BlocoP()
    {
        var atributo = typeof(RegistroP100).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("P100");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("P");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroP100Com11CamposNaOrdem()
    {
        _catalogo.TentarObter("P100".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("P100");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "DtIni", "DtFin", "VlRecTotEst", "CodAtivEcon", "VlRecAtivEstab",
            "VlExc", "VlBcCont", "AliqCont", "VlContApu", "CodCta", "InfoCompl",
        ]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // DtIni
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // DtFin
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // VlRecTotEst
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // CodAtivEcon
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // VlRecAtivEstab
        meta.Campos[5].Obrigatorio.Should().BeFalse();  // VlExc
        meta.Campos[6].Obrigatorio.Should().BeTrue();   // VlBcCont
        meta.Campos[7].Obrigatorio.Should().BeTrue();   // AliqCont
        meta.Campos[8].Obrigatorio.Should().BeTrue();   // VlContApu
        meta.Campos[9].Obrigatorio.Should().BeFalse();  // CodCta
        meta.Campos[10].Obrigatorio.Should().BeFalse(); // InfoCompl
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("P100".AsSpan(), out var meta);
        var registro = (RegistroP100)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01012023".AsSpan());           // DtIni
        meta.Campos[1].Definidor(registro, "31012023".AsSpan());           // DtFin
        meta.Campos[2].Definidor(registro, "100000,00".AsSpan());          // VlRecTotEst
        meta.Campos[3].Definidor(registro, "00000001".AsSpan());           // CodAtivEcon
        meta.Campos[4].Definidor(registro, "80000,00".AsSpan());           // VlRecAtivEstab
        meta.Campos[5].Definidor(registro, "5000,00".AsSpan());            // VlExc
        meta.Campos[6].Definidor(registro, "75000,00".AsSpan());           // VlBcCont
        meta.Campos[7].Definidor(registro, "3,0000".AsSpan());             // AliqCont
        meta.Campos[8].Definidor(registro, "2250,00".AsSpan());            // VlContApu
        meta.Campos[9].Definidor(registro, "1.2.3.001".AsSpan());          // CodCta
        meta.Campos[10].Definidor(registro, "Info complementar".AsSpan()); // InfoCompl

        registro.DtIni.Should().Be(new DateOnly(2023, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2023, 1, 31));
        registro.VlRecTotEst.Should().Be(100000m);
        registro.CodAtivEcon.Should().Be("00000001");
        registro.VlRecAtivEstab.Should().Be(80000m);
        registro.VlExc.Should().Be(5000m);
        registro.VlBcCont.Should().Be(75000m);
        registro.AliqCont.Should().Be(3m);
        registro.VlContApu.Should().Be(2250m);
        registro.CodCta.Should().Be("1.2.3.001");
        registro.InfoCompl.Should().Be("Info complementar");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("P100".AsSpan(), out var meta);
        var registro = (RegistroP100)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlExc
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCta
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty); // InfoCompl

        registro.VlExc.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.InfoCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|P100|01012023|31012023|100000,00|00000001|80000,00|5000,00|75000,00|3,0000|2250,00|1.2.3.001|Info complementar|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped =
            "|P100|01012023|31012023|100000,00|00000001|80000,00||75000,00|3,0000|2250,00|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
