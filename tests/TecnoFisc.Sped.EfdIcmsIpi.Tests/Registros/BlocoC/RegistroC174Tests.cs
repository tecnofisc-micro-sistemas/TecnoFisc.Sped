using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.057 — exercita a forma do <see cref="RegistroC174"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 84): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC174Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC174).Assembly);

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
    public void Atributo_DeclaraC174_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC174).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C174");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC174Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("C174".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C174");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndArm", "NumArm", "DescrCompl",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 3));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C174".AsSpan(), out var meta);
        var registro = (RegistroC174)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());                    // IndArm
        meta.Campos[1].Definidor(registro, "SN12345".AsSpan());              // NumArm
        meta.Campos[2].Definidor(registro, "Pistola cal. 9mm".AsSpan());     // DescrCompl

        registro.IndArm.Should().Be(IndicadorTipoArmaFogo.UsoRestrito);
        registro.NumArm.Should().Be("SN12345");
        registro.DescrCompl.Should().Be("Pistola cal. 9mm");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C174".AsSpan(), out var meta);
        var registro = (RegistroC174)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.IndArm.Should().BeNull();
        registro.NumArm.Should().BeNull();
        registro.DescrCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Arma de uso restrito, série SN12345, descrição completa.
        const string sped = "|C174|1|SN12345|Pistola cal. 9mm cano 4 pol.|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposVazios_PreservaTextoCanonico()
    {
        // Todos os campos opcionais ausentes.
        const string sped = "|C174||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorTipoArmaFogo.UsoPermitido)]
    [InlineData("1", IndicadorTipoArmaFogo.UsoRestrito)]
    public void Definidor_IndArm_CobertosEnum(string valor, IndicadorTipoArmaFogo esperado)
    {
        _catalogo.TentarObter("C174".AsSpan(), out var meta);
        var registro = (RegistroC174)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, valor.AsSpan());

        registro.IndArm.Should().Be(esperado);
    }
}
