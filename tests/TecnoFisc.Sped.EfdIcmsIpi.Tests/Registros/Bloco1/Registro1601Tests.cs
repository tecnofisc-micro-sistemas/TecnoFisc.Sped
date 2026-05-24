using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.016.001 — exercita a forma do <see cref="Registro1601"/> contra o Guia Prático
/// EFD-ICMS/IPI v3.2.2 (p. 305): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro1601Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1601).Assembly);

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
    public void Atributo_Declara1601_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1601).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1601");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
        atributo.IntroduzidoEm.Should().Be((int)LayoutEfdIcmsIpi.V016);
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1601Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("1601".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1601");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodPartIp", "CodPartIt", "TotVs", "TotIss", "TotOutros"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 5));
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "CodPartIp", "TotVs", "TotIss", "TotOutros"
        ]);
        meta.Campos[0].Tamanho.Should().Be(60);
        meta.Campos[1].Tamanho.Should().Be(60);
        meta.Campos[2].Decimais.Should().Be(2);
        meta.Campos[3].Decimais.Should().Be(2);
        meta.Campos[4].Decimais.Should().Be(2);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1601".AsSpan(), out var meta);
        var registro = (Registro1601)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "PAGTO001".AsSpan());
        meta.Campos[1].Definidor(registro, "INTERM001".AsSpan());
        meta.Campos[2].Definidor(registro, "12345,67".AsSpan());
        meta.Campos[3].Definidor(registro, "2000,00".AsSpan());
        meta.Campos[4].Definidor(registro, "500,00".AsSpan());

        registro.CodPartIp.Should().Be("PAGTO001");
        registro.CodPartIt.Should().Be("INTERM001");
        registro.TotVs.Should().Be(12345.67m);
        registro.TotIss.Should().Be(2000.00m);
        registro.TotOutros.Should().Be(500.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1601".AsSpan(), out var meta);
        var registro = (Registro1601)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodPartIp.Should().BeNull();
        registro.CodPartIt.Should().BeNull();
        registro.TotVs.Should().Be(0m);
        registro.TotIss.Should().Be(0m);
        registro.TotOutros.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1601|PAGTO001|INTERM001|12345,67|2000,00|500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemIntermediador_PreservaTextoCanonico()
    {
        const string sped = "|1601|PAGTO001||9999,99|0,00|100,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
