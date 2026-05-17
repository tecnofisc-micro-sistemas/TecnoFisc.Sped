using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.150 — exercita a forma do <see cref="RegistroD695"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (pp. 202-203): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD695Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD695).Assembly);

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
    public void Atributo_DeclaraD695_Nivel2_BlocoD()
    {
        var atributo = typeof(RegistroD695).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D695");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD695Com8CamposNaOrdem()
    {
        _catalogo.TentarObter("D695".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D695");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMod", "Ser", "NroOrdIni", "NroOrdFin",
            "DtDocIni", "DtDocFin", "NomMest", "ChvCodDig",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D695".AsSpan(), out var meta);
        var registro = (RegistroD695)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "21".AsSpan());                                // CodMod
        meta.Campos[1].Definidor(registro, "0001".AsSpan());                              // Ser
        meta.Campos[2].Definidor(registro, "1".AsSpan());                                 // NroOrdIni
        meta.Campos[3].Definidor(registro, "500".AsSpan());                               // NroOrdFin
        meta.Campos[4].Definidor(registro, "01012025".AsSpan());                          // DtDocIni
        meta.Campos[5].Definidor(registro, "31012025".AsSpan());                          // DtDocFin
        meta.Campos[6].Definidor(registro, "MESTRE_D695_01012025.txt".AsSpan());          // NomMest
        meta.Campos[7].Definidor(registro, "ABCDEF1234567890ABCDEF1234567890".AsSpan());  // ChvCodDig

        registro.CodMod.Should().Be("21");
        registro.Ser.Should().Be("0001");
        registro.NroOrdIni.Should().Be(1);
        registro.NroOrdFin.Should().Be(500);
        registro.DtDocIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtDocFin.Should().Be(new DateOnly(2025, 1, 31));
        registro.NomMest.Should().Be("MESTRE_D695_01012025.txt");
        registro.ChvCodDig.Should().Be("ABCDEF1234567890ABCDEF1234567890");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D695".AsSpan(), out var meta);
        var registro = (RegistroD695)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodMod.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.NroOrdIni.Should().BeNull();
        registro.NroOrdFin.Should().BeNull();
        registro.DtDocIni.Should().BeNull();
        registro.DtDocFin.Should().BeNull();
        registro.NomMest.Should().BeNull();
        registro.ChvCodDig.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // COD_MOD 21 (serviço comunicação), série 0001, intervalo NF 1-500, jan/2025.
        const string sped =
            "|D695|21|0001|1|500|01012025|31012025|MESTRE_D695_01012025.txt|ABCDEF1234567890ABCDEF1234567890|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCodMod22ETelecomunicacao_PreservaTextoCanonico()
    {
        // COD_MOD 22 (telecomunicação), fevereiro/2025, sem chave de codificação.
        const string sped =
            "|D695|22|0002|501|1000|01022025|28022025|MESTRE_D695_TEL.txt||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComTodosCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped = "|D695|||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
