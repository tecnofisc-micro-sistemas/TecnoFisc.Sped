using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.051 — exercita a forma do <see cref="RegistroC160"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 75): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC160Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC160).Assembly);

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
    public void Atributo_DeclaraC160_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC160).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C160");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC160Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("C160".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C160");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodPart", "VeicId", "QtdVol", "PesoBrt", "PesoLiq", "UfId",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 6));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C160".AsSpan(), out var meta);
        var registro = (RegistroC160)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "TRANSP001".AsSpan());  // CodPart
        meta.Campos[1].Definidor(registro, "ABC1234".AsSpan());    // VeicId
        meta.Campos[2].Definidor(registro, "10".AsSpan());         // QtdVol
        meta.Campos[3].Definidor(registro, "1500.50".AsSpan());    // PesoBrt
        meta.Campos[4].Definidor(registro, "1200.30".AsSpan());    // PesoLiq
        meta.Campos[5].Definidor(registro, "SP".AsSpan());         // UfId

        registro.CodPart.Should().Be("TRANSP001");
        registro.VeicId.Should().Be("ABC1234");
        registro.QtdVol.Should().Be(10);
        registro.PesoBrt.Should().Be(1500.50m);
        registro.PesoLiq.Should().Be(1200.30m);
        registro.UfId.Should().Be("SP");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C160".AsSpan(), out var meta);
        var registro = (RegistroC160)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty); // CodPart
        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty); // VeicId
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // QtdVol
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // PesoBrt
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // PesoLiq
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // UfId

        registro.CodPart.Should().BeNull();
        registro.VeicId.Should().BeNull();
        registro.QtdVol.Should().BeNull();
        registro.PesoBrt.Should().BeNull();
        registro.PesoLiq.Should().BeNull();
        registro.UfId.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Volumes transportados com todos os campos preenchidos.
        const string sped = "|C160|TRANSP001|ABC1234|10|1500,50|1200,30|SP|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemTransportadorNemUf_PreservaTextoCanonico()
    {
        // Transportador é o próprio emitente: COD_PART e UF_ID vazios.
        const string sped = "|C160|||5|2000,00|1800,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
