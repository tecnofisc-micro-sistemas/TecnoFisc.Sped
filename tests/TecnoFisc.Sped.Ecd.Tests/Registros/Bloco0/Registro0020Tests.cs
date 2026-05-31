using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 10.004 — exercita a forma do <see cref="Registro0020"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 78): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class Registro0020Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

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
    public void Atributo_Declara0020_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0020).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0020");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0020ComSeteCamposNaOrdem()
    {
        _catalogo.TentarObter("0020".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0020");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["IndDec", "Cnpj", "Uf", "Ie", "CodMun", "Im", "Nire"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0020".AsSpan(), out var meta);
        var registro = (Registro0020)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());
        meta.Campos[1].Definidor(registro, "11111111000191".AsSpan());
        meta.Campos[2].Definidor(registro, "DF".AsSpan());
        meta.Campos[3].Definidor(registro, "123456".AsSpan());
        meta.Campos[4].Definidor(registro, "3434401".AsSpan());
        meta.Campos[5].Definidor(registro, "".AsSpan());
        meta.Campos[6].Definidor(registro, "11111111".AsSpan());

        registro.IndDec.Should().Be(IndicadorDescentralizacao.EscrituradorFilial);
        registro.Cnpj.Should().NotBeNull();
        registro.Cnpj!.ToString().Should().Be("11111111000191");
        registro.Uf.Should().Be("DF");
        registro.Ie.Should().Be("123456");
        registro.CodMun.Should().Be("3434401");
        registro.Im.Should().BeNull();
        registro.Nire.Should().Be(11111111L);
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("0020".AsSpan(), out var meta);
        var registro = (Registro0020)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // Ie
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // CodMun
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // Im
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // Nire

        registro.Ie.Should().BeNull();
        registro.CodMun.Should().BeNull();
        registro.Im.Should().BeNull();
        registro.Nire.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual ECD leiaute 9, p. 81
        const string sped = "|0020|1|11111111000191|DF|123456|3434401||11111111|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_EscrituradorMatriz_PreservaTextoCanonico()
    {
        const string sped = "|0020|0|11111111000191|SP|123456789|3550308|ISS-2345||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped = "|0020|0|11111111000191|RJ|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
