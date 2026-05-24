using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.109 — exercita a forma do <see cref="RegistroC860"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 158): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC860Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC860).Assembly);

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
    public void Atributo_DeclaraC860_Nivel2_BlocoC()
    {
        var atributo = typeof(RegistroC860).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C860");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC860Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("C860".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C860");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMod", "NrSat", "DtDoc",
            "DocIni", "DocFim"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C860".AsSpan(), out var meta);
        var registro = (RegistroC860)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "59".AsSpan());           // CodMod
        meta.Campos[1].Definidor(registro, "900004050".AsSpan());    // NrSat
        meta.Campos[2].Definidor(registro, "01012024".AsSpan());     // DtDoc
        meta.Campos[3].Definidor(registro, "1".AsSpan());            // DocIni
        meta.Campos[4].Definidor(registro, "100".AsSpan());          // DocFim

        registro.CodMod.Should().Be("59");
        registro.NrSat.Should().Be(900004050);
        registro.DtDoc.Should().Be(new DateOnly(2024, 1, 1));
        registro.DocIni.Should().Be(1);
        registro.DocFim.Should().Be(100);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C860".AsSpan(), out var meta);
        var registro = (RegistroC860)meta!.Fabrica();

        // CodMod (índice 0) é o único campo nullable.
        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodMod.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C860|59|900004050|01012024|1|100|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DocumentoInicialIgualFinal_PreservaTextoCanonico()
    {
        // Equipamento com apenas um CF-e-SAT emitido no período.
        const string sped =
            "|C860|59|100000001|15062023|42|42|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
