using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.116 — exercita a forma do <see cref="RegistroD101"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 168-169): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD101Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD101).Assembly);

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
    public void Atributo_DeclaraD101_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD101).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D101");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD101Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("D101".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D101");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "VlFcpUfDest", "VlIcmsUfDest", "VlIcmsUfRem",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 3));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D101".AsSpan(), out var meta);
        var registro = (RegistroD101)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "10.00".AsSpan());   // VlFcpUfDest
        meta.Campos[1].Definidor(registro, "40.00".AsSpan());   // VlIcmsUfDest
        meta.Campos[2].Definidor(registro, "60.00".AsSpan());   // VlIcmsUfRem

        registro.VlFcpUfDest.Should().Be(10.00m);
        registro.VlIcmsUfDest.Should().Be(40.00m);
        registro.VlIcmsUfRem.Should().Be(60.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        // Todos os campos são obrigatórios (decimal não nullable); vazio resulta em zero.
        _catalogo.TentarObter("D101".AsSpan(), out var meta);
        var registro = (RegistroD101)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);
        registro.VlFcpUfDest.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CT-e interestadual (cód. 57) a consumidor final não contribuinte EC 87/15: FCP=10, ICMS destino=40, ICMS remetente=60.
        const string sped = "|D101|10,00|40,00|60,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresZerados_PreservaTextoCanonico()
    {
        // Prestação sem FCP, apenas ICMS interestadual.
        const string sped = "|D101|0,00|120,00|80,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
