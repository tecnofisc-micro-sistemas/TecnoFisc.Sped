using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco9;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco9;

/// <summary>
/// Sub-stage 8.254 - exercita a forma do <see cref="Registro9990"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 302): metadados de catalogo, mapeamento de campos e invariante
/// de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro9990Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro9990).Assembly);

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
    public void Atributo_Declara9990_Nivel1_Bloco9()
    {
        var atributo = typeof(Registro9990).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("9990");
        atributo.Nivel.Should().Be(1);
        atributo.Bloco.Should().Be("9");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro9990ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("9990".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("9990");
        meta.Campos.Select(c => c.Nome).Should().Equal(["QtdLin9"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("9990".AsSpan(), out var meta);
        var registro = (Registro9990)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "4".AsSpan());

        registro.QtdLin9.Should().Be(4);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("9990".AsSpan(), out var meta);
        var registro = (Registro9990)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.QtdLin9.Should().Be(0);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|9990|4|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_Bloco9ComVariosTotalizadores_PreservaTextoCanonico()
    {
        const string sped = "|9990|258|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
