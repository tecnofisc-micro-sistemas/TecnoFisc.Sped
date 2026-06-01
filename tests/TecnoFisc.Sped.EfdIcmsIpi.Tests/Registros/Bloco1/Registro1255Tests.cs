using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.223 - exercita a forma do <see cref="Registro1255"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 274-275): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1255Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1255).Assembly);

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
    public void Atributo_Declara1255_Nivel3_Bloco1()
    {
        var atributo = typeof(Registro1255).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1255");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1255ComSeisCamposNaOrdem()
    {
        _catalogo.TentarObter("1255".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1255");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMotRestCompl",
            "VlCreditoIcmsOpMot",
            "VlIcmsStRestMot",
            "VlFcpStRestMot",
            "VlIcmsStComplMot",
            "VlFcpStComplMot",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 6));
        meta.Campos.Should().OnlyContain(c => c.Obrigatorio);
        meta.Campos[0].Tamanho.Should().Be(5);
        meta.Campos.Skip(1).Should().OnlyContain(c => c.Tamanho == 0);
        meta.Campos.Skip(1).Should().OnlyContain(c => c.Decimais == 2);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1255".AsSpan(), out var meta);
        var registro = (Registro1255)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "00507".AsSpan());
        meta.Campos[1].Definidor(registro, "100,10".AsSpan());
        meta.Campos[2].Definidor(registro, "200,20".AsSpan());
        meta.Campos[3].Definidor(registro, "30,30".AsSpan());
        meta.Campos[4].Definidor(registro, "400,40".AsSpan());
        meta.Campos[5].Definidor(registro, "50,50".AsSpan());

        registro.CodMotRestCompl.Should().Be("00507");
        registro.VlCreditoIcmsOpMot.Should().Be(100.10m);
        registro.VlIcmsStRestMot.Should().Be(200.20m);
        registro.VlFcpStRestMot.Should().Be(30.30m);
        registro.VlIcmsStComplMot.Should().Be(400.40m);
        registro.VlFcpStComplMot.Should().Be(50.50m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNuloOuZero()
    {
        _catalogo.TentarObter("1255".AsSpan(), out var meta);
        var registro = (Registro1255)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.CodMotRestCompl.Should().BeNull();
        registro.VlCreditoIcmsOpMot.Should().Be(0m);
        registro.VlIcmsStRestMot.Should().Be(0m);
        registro.VlFcpStRestMot.Should().Be(0m);
        registro.VlIcmsStComplMot.Should().Be(0m);
        registro.VlFcpStComplMot.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1255|00507|100,10|200,20|30,30|400,40|50,50|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SaldosZerados_PreservaTextoCanonico()
    {
        const string sped = "|1255|00100|0,00|0,00|0,00|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
