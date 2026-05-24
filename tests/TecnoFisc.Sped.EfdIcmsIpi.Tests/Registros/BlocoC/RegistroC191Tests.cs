using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.068 — exercita a forma do <see cref="RegistroC191"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 104): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC191Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC191).Assembly);

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
    public void Atributo_DeclaraC191_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC191).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C191");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC191Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("C191".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C191");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "VlFcpOp", "VlFcpSt", "VlFcpRet"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C191".AsSpan(), out var meta);
        var registro = (RegistroC191)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "100,00".AsSpan());   // VlFcpOp
        meta.Campos[1].Definidor(registro, "50,00".AsSpan());    // VlFcpSt
        meta.Campos[2].Definidor(registro, "25,00".AsSpan());    // VlFcpRet

        registro.VlFcpOp.Should().Be(100.00m);
        registro.VlFcpSt.Should().Be(50.00m);
        registro.VlFcpRet.Should().Be(25.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C191".AsSpan(), out var meta);
        var registro = (RegistroC191)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.VlFcpOp.Should().BeNull();
        registro.VlFcpSt.Should().BeNull();
        registro.VlFcpRet.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C191|100,00|50,00|25,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposVazios_PreservaTextoCanonico()
    {
        // Todos OC: registro válido com todos os campos em branco.
        const string sped =
            "|C191||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComFcpOpApenas_PreservaTextoCanonico()
    {
        // Operação própria com FCP; ST e RET ausentes (CST x00, x10, x20, x51, x70 ou x90).
        const string sped =
            "|C191|200,00|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
