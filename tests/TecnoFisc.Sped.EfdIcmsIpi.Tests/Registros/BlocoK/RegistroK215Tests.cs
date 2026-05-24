using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.197 — exercita a forma do <see cref="RegistroK215"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 252-253): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK215Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK215).Assembly);

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
    public void Atributo_DeclaraK215_Nivel4_BlocoK()
    {
        var atributo = typeof(RegistroK215).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K215");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK215ComDoisCamposNaOrdem()
    {
        _catalogo.TentarObter("K215".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K215");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodItemDes",
            "QtdDes",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K215".AsSpan(), out var meta);
        var registro = (RegistroK215)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "ITEM-DES".AsSpan());
        meta.Campos[1].Definidor(registro, "7,654321".AsSpan());

        registro.CodItemDes.Should().Be("ITEM-DES");
        registro.QtdDes.Should().Be(7.654321m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K215".AsSpan(), out var meta);
        var registro = (RegistroK215)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodItemDes.Should().BeNull();
        registro.QtdDes.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K215|ITEM-DES|7,654321|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComQuantidadeZero_PreservaTextoCanonico()
    {
        const string sped = "|K215|ITEM-DES-ZERO|0,000000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
