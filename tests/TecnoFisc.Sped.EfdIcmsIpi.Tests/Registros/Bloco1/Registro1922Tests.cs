using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.243 - exercita a forma do <see cref="Registro1922"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 293): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1922Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1922).Assembly);

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
    public void Atributo_Declara1922_Nivel6_Bloco1()
    {
        var atributo = typeof(Registro1922).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1922");
        atributo.Nivel.Should().Be(6);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1922Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("1922".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1922");
        meta.Campos.Select(c => c.Nome).Should().Equal(["NumDa", "NumProc", "IndProc", "Proc", "TxtCompl"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
        meta.Campos.Select(c => c.Tamanho).Should().Equal([0, 15, 1, 0, 0]);
        meta.Campos.Select(c => c.Decimais).Should().Equal([0, 0, 0, 0, 0]);
        meta.Campos.Where(c => c.Obrigatorio).Should().BeEmpty();
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1922".AsSpan(), out var meta);
        var registro = (Registro1922)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "DAE20230001".AsSpan());
        meta.Campos[1].Definidor(registro, "SP202300001".AsSpan());
        meta.Campos[2].Definidor(registro, "1".AsSpan());
        meta.Campos[3].Definidor(registro, "Processo judicial".AsSpan());
        meta.Campos[4].Definidor(registro, "Informacao adicional".AsSpan());

        registro.NumDa.Should().Be("DAE20230001");
        registro.NumProc.Should().Be("SP202300001");
        registro.IndProc.Should().Be(IndicadorOrigemProcesso.JusticaFederal);
        registro.Proc.Should().Be("Processo judicial");
        registro.TxtCompl.Should().Be("Informacao adicional");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1922".AsSpan(), out var meta);
        var registro = (Registro1922)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.NumDa.Should().BeNull();
        registro.NumProc.Should().BeNull();
        registro.IndProc.Should().BeNull();
        registro.Proc.Should().BeNull();
        registro.TxtCompl.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorOrigemProcesso.Sefaz)]
    [InlineData("1", IndicadorOrigemProcesso.JusticaFederal)]
    [InlineData("2", IndicadorOrigemProcesso.JusticaEstadual)]
    [InlineData("9", IndicadorOrigemProcesso.Outros)]
    [InlineData("", null)]
    public void Definidor_IndProc_MapeiaCodigos(string input, IndicadorOrigemProcesso? esperado)
    {
        _catalogo.TentarObter("1922".AsSpan(), out var meta);
        var registro = (Registro1922)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, input.AsSpan());

        registro.IndProc.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|1922|DAE20230001|SP202300001|1|Processo judicial|Informacao adicional|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_TodosCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped = "|1922||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
