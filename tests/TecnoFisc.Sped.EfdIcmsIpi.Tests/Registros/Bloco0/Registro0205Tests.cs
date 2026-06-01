using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.011 — exercita a forma do <see cref="Registro0205"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 35-36): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0205Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0205).Assembly);

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
    public void Atributo_Declara0205_Nivel3_Bloco0()
    {
        var atributo = typeof(Registro0205).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0205");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0205Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("0205".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0205");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "DescrAntItem",
            "DtIni",
            "DtFim",
            "CodAntItem",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0205".AsSpan(), out var meta);
        var registro = (Registro0205)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "Descricao Anterior do Produto".AsSpan());
        meta.Campos[1].Definidor(registro, "01012023".AsSpan());
        meta.Campos[2].Definidor(registro, "31122023".AsSpan());
        meta.Campos[3].Definidor(registro, "COD-ANT-001".AsSpan());

        registro.DescrAntItem.Should().Be("Descricao Anterior do Produto");
        registro.DtIni.Should().Be(new DateOnly(2023, 1, 1));
        registro.DtFim.Should().Be(new DateOnly(2023, 12, 31));
        registro.CodAntItem.Should().Be("COD-ANT-001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0205".AsSpan(), out var meta);
        var registro = (Registro0205)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);
        meta.Campos[3].Definidor(registro, Span<char>.Empty);

        registro.DescrAntItem.Should().BeNull();
        registro.CodAntItem.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0205|Descricao Anterior do Produto|01012023|31122023|COD-ANT-001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComDescrAntItemApenas_PreservaTextoCanonico()
    {
        const string sped = "|0205|Produto Renomeado|01062024|30062024||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCodAntItemApenas_PreservaTextoCanonico()
    {
        const string sped = "|0205||01012024|31122024|CODIGO-ANTIGO|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
