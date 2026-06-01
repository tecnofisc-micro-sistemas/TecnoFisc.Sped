using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco9;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco9;

/// <summary>
/// Sub-stage 8.255 - exercita a forma do <see cref="Registro9999"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 303): metadados de catalogo, mapeamento de campos e invariante
/// de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro9999Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro9999).Assembly);

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
    public void Atributo_Declara9999_Nivel0_Bloco9()
    {
        var atributo = typeof(Registro9999).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("9999");
        atributo.Nivel.Should().Be(0);
        atributo.Bloco.Should().Be("9");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro9999ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("9999".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("9999");
        meta.Campos.Select(c => c.Nome).Should().Equal(["QtdLin"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("9999".AsSpan(), out var meta);
        var registro = (Registro9999)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "258".AsSpan());

        registro.QtdLin.Should().Be(258);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("9999".AsSpan(), out var meta);
        var registro = (Registro9999)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.QtdLin.Should().Be(0);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|9999|258|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ArquivoMinimo_PreservaTextoCanonico()
    {
        const string sped = "|9999|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
