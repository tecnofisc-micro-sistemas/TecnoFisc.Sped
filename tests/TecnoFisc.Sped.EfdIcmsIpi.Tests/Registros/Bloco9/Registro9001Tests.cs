using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco9;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco9;

/// <summary>
/// Sub-stage 8.252 - exercita a forma do <see cref="Registro9001"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 302): metadados de catalogo, mapeamento de campos e invariante
/// de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro9001Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro9001).Assembly);

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
    public void Atributo_Declara9001_Nivel1_Bloco9()
    {
        var atributo = typeof(Registro9001).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("9001");
        atributo.Nivel.Should().Be(1);
        atributo.Bloco.Should().Be("9");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro9001ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("9001".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("9001");
        meta.Campos.Select(c => c.Nome).Should().Equal(["IndMov"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
        meta.Campos[0].Tamanho.Should().Be(1);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("9001".AsSpan(), out var meta);
        var registro = (Registro9001)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());

        registro.IndMov.Should().Be(IndicadorMovimentoBloco.ComDados);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("9001".AsSpan(), out var meta);
        var registro = (Registro9001)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.IndMov.Should().Be(default(IndicadorMovimentoBloco));
    }

    [Theory]
    [InlineData(IndicadorMovimentoBloco.ComDados, "0")]
    [InlineData(IndicadorMovimentoBloco.SemDados, "1")]
    public void Serializar_IndMov_RetornaCodigo(IndicadorMovimentoBloco movimento, string esperado)
    {
        _catalogo.TentarObter("9001".AsSpan(), out var meta);
        var registro = (Registro9001)meta!.Fabrica();
        registro.IndMov = movimento;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|9001|0|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_BlocoSemDados_PreservaTextoCanonico()
    {
        const string sped = "|9001|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
