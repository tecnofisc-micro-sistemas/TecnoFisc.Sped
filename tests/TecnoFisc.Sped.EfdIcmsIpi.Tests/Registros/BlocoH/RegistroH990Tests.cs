using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoH;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoH;

/// <summary>
/// Sub-stage 8.192 - exercita a forma do <see cref="RegistroH990"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 249): metadados de catalogo, mapeamento de campos e invariante
/// de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class RegistroH990Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroH990).Assembly);

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
    public void Atributo_DeclaraH990_Nivel1_BlocoH()
    {
        var atributo = typeof(RegistroH990).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("H990");
        atributo.Nivel.Should().Be(1);
        atributo.Bloco.Should().Be("H");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroH990Com1CampoNaOrdem()
    {
        _catalogo.TentarObter("H990".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("H990");
        meta.Campos.Select(c => c.Nome).Should().Equal(["QtdLinH"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("H990".AsSpan(), out var meta);
        var registro = (RegistroH990)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "42".AsSpan());

        registro.QtdLinH.Should().Be(42);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("H990".AsSpan(), out var meta);
        var registro = (RegistroH990)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.QtdLinH.Should().Be(0);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|H990|42|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_LinhaUnica_PreservaTextoCanonico()
    {
        const string sped = "|H990|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
