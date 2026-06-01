using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.208 — exercita a forma do <see cref="RegistroK290"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 264): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK290Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK290).Assembly);

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
    public void Atributo_DeclaraK290_Nivel3_BlocoK()
    {
        var atributo = typeof(RegistroK290).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K290");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK290ComTresCamposNaOrdem()
    {
        _catalogo.TentarObter("K290".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K290");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtIniOp",
            "DtFinOp",
            "CodDocOp",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K290".AsSpan(), out var meta);
        var registro = (RegistroK290)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01012025".AsSpan());
        meta.Campos[1].Definidor(registro, "31012025".AsSpan());
        meta.Campos[2].Definidor(registro, "OP-CONJ-001".AsSpan());

        registro.DtIniOp.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFinOp.Should().Be(new DateOnly(2025, 1, 31));
        registro.CodDocOp.Should().Be("OP-CONJ-001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K290".AsSpan(), out var meta);
        var registro = (RegistroK290)meta!.Fabrica();

        meta!.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.DtIniOp.Should().BeNull();
        registro.DtFinOp.Should().BeNull();
        registro.CodDocOp.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K290|01012025|31012025|OP-CONJ-001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemOrdemDeProducao_PreservaTextoCanonico()
    {
        const string sped = "|K290||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
