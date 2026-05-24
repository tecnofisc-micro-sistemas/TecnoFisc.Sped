using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.110 — exercita a forma do <see cref="RegistroC870"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 158): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC870Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC870).Assembly);

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
    public void Atributo_DeclaraC870_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC870).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C870");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC870Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("C870".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C870");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodItem", "Qtd", "Unid", "CstIcms", "Cfop"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C870".AsSpan(), out var meta);
        var registro = (RegistroC870)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "PROD001".AsSpan());  // CodItem
        meta.Campos[1].Definidor(registro, "10,00000".AsSpan()); // Qtd
        meta.Campos[2].Definidor(registro, "UN".AsSpan());       // Unid
        meta.Campos[3].Definidor(registro, "60".AsSpan());       // CstIcms
        meta.Campos[4].Definidor(registro, "5102".AsSpan());     // Cfop

        registro.CodItem.Should().Be("PROD001");
        registro.Qtd.Should().Be(10.00000m);
        registro.Unid.Should().Be("UN");
        registro.CstIcms.Should().Be(60);
        registro.Cfop.Should().Be(Cfop.Create("5102".AsSpan()));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C870".AsSpan(), out var meta);
        var registro = (RegistroC870)meta!.Fabrica();

        // Qtd (índice 1) é não-nullable — não testado aqui.
        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty); // CodItem
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // Unid
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // CstIcms
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // Cfop

        registro.CodItem.Should().BeNull();
        registro.Unid.Should().BeNull();
        registro.CstIcms.Should().BeNull();
        registro.Cfop.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C870|PROD001|10,00000|UN|60|5102|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCstIcmsZero_PreservaTextoCanonico()
    {
        // CST_ICMS = 0 deve serializar como "0", não string vazia.
        const string sped =
            "|C870|MERCH002|2,00000|CX|0|5405|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
