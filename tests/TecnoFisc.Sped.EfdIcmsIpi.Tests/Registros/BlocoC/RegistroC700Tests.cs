using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.102 — exercita a forma do <see cref="RegistroC700"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (pp. 148-149): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC700Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC700).Assembly);

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
    public void Atributo_DeclaraC700_Nivel2_BlocoC()
    {
        var atributo = typeof(RegistroC700).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C700");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC700Com8CamposNaOrdem()
    {
        _catalogo.TentarObter("C700".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C700");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMod", "Ser", "NroOrdIni", "NroOrdFin",
            "DtDocIni", "DtDocFin", "NomMest", "ChvCodDig"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C700".AsSpan(), out var meta);
        var registro = (RegistroC700)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "06".AsSpan());                                 // CodMod
        meta.Campos[1].Definidor(registro, "A".AsSpan());                                  // Ser
        meta.Campos[2].Definidor(registro, "1".AsSpan());                                   // NroOrdIni
        meta.Campos[3].Definidor(registro, "100".AsSpan());                                 // NroOrdFin
        meta.Campos[4].Definidor(registro, "01012023".AsSpan());                            // DtDocIni
        meta.Campos[5].Definidor(registro, "31012023".AsSpan());                            // DtDocFin
        meta.Campos[6].Definidor(registro, "ARQUIVO_MESTRE.TXT".AsSpan());                 // NomMest
        meta.Campos[7].Definidor(registro, "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456".AsSpan());   // ChvCodDig

        registro.CodMod.Should().Be("06");
        registro.Ser.Should().Be("A");
        registro.NroOrdIni.Should().Be(1L);
        registro.NroOrdFin.Should().Be(100L);
        registro.DtDocIni.Should().Be(new DateOnly(2023, 1, 1));
        registro.DtDocFin.Should().Be(new DateOnly(2023, 1, 31));
        registro.NomMest.Should().Be("ARQUIVO_MESTRE.TXT");
        registro.ChvCodDig.Should().Be("ABCDEFGHIJKLMNOPQRSTUVWXYZ123456");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C700".AsSpan(), out var meta);
        var registro = (RegistroC700)meta!.Fabrica();

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
        const string sped =
            "|C700|06|A|1|100|01012023|31012023|ARQUIVO_MESTRE.TXT|ABCDEFGHIJKLMNOPQRSTUVWXYZ123456|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ModeloGasSemSerie_PreservaTextoCanonico()
    {
        // Gás canalizado (COD_MOD=28), SER vazio.
        const string sped =
            "|C700|28||1|50|01022023|28022023|GAS_MESTRE.TXT|ZYXWVUTSRQPONMLKJIHGFEDCBA987654|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
