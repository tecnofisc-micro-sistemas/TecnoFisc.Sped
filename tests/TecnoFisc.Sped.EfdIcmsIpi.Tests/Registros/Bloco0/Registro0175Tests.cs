using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.008 — exercita a forma do <see cref="Registro0175"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 32): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0175Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0175).Assembly);

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
    public void Atributo_Declara0175_Nivel3_Bloco0()
    {
        var atributo = typeof(Registro0175).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0175");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0175Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("0175".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0175");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "DtAlt",
            "NrCampo",
            "ContAnt",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0175".AsSpan(), out var meta);
        var registro = (Registro0175)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "15012024".AsSpan());
        meta.Campos[1].Definidor(registro, "03".AsSpan());
        meta.Campos[2].Definidor(registro, "Empresa Antiga Ltda".AsSpan());

        registro.DtAlt.Should().Be(new DateOnly(2024, 1, 15));
        registro.NrCampo.Should().Be("03");
        registro.ContAnt.Should().Be("Empresa Antiga Ltda");
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0175|15012024|03|Empresa Antiga Ltda|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AlteracaoCnpj_PreservaTextoCanonico()
    {
        const string sped = "|0175|20062023|05|12345678000195|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
