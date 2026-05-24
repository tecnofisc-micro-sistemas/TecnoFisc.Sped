using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Exercita o <see cref="Registro0220"/> contra o Guia Prático EFD-ICMS/IPI v3.2.2 (p. 41):
/// metadados de catálogo, mapeamento de campos e invariante de round-trip parse → gerar → texto
/// idêntico. Campo 04 <c>COD_BARRA</c> introduzido em V016 (sub-stage 8.016.002).
/// </summary>
public sealed class Registro0220Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0220).Assembly);

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
    public void Atributo_Declara0220_Nivel3_Bloco0()
    {
        var atributo = typeof(Registro0220).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0220");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0220Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("0220".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0220");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "UnidConv",
            "FatConv",
            "CodBarra",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void CodBarra_DesdeVersaoV016()
    {
        _catalogo.TentarObter("0220".AsSpan(), out var meta);
        var campoCodBarra = meta!.Campos.Single(c => c.Nome == "CodBarra");

        campoCodBarra.DesdeVersao.Should().Be((int)LayoutEfdIcmsIpi.V016);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0220".AsSpan(), out var meta);
        var registro = (Registro0220)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "UN".AsSpan());
        meta.Campos[1].Definidor(registro, "12.500000".AsSpan());
        meta.Campos[2].Definidor(registro, "7891234567893".AsSpan());

        registro.UnidConv.Should().Be("UN");
        registro.FatConv.Should().Be(12.5m);
        registro.CodBarra.Should().Be("7891234567893");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0220".AsSpan(), out var meta);
        var registro = (Registro0220)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.UnidConv.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0220|UN|12,500000|7891234567893|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComFatorUnitario_PreservaTextoCanonico()
    {
        const string sped = "|0220|CX|1,000000||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCodBarraGtin14_PreservaTextoCanonico()
    {
        const string sped = "|0220|PC|0,500000|07891234567890|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCodBarra_EmiteFieldVazio()
    {
        // Arquivos V015 (3 pipes) são lidos sem COD_BARRA; ao regerar emite campo vazio.
        const string entrada = "|0220|UN|12,500000|\r\n";
        const string esperado = "|0220|UN|12,500000||\r\n";

        var resultado = await RoundTripAsync(entrada, TestContext.Current.CancellationToken);

        resultado.Should().Be(esperado);
    }
}
