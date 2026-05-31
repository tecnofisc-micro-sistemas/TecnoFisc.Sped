using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.023 — exercita a forma do <see cref="RegistroI020"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 110–113): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI020Tests
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
    public void Atributo_DeclaraI020_Nivel3_BlocoI()
    {
        var atributo = typeof(RegistroI020).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I020");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI020ComCincoCamposNaOrdem()
    {
        _catalogo.TentarObter("I020".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I020");
        meta.Campos.Select(c => c.Nome).Should().Equal(["RegCod", "NumAd", "Campo", "Descricao", "Tipo"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I020".AsSpan(), out var meta);
        var registro = (RegistroI020)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "I310".AsSpan());
        meta.Campos[1].Definidor(registro, "6".AsSpan());
        meta.Campos[2].Definidor(registro, "VAL_DEB_MF".AsSpan());
        meta.Campos[3].Definidor(registro, "TOTAL DOS DEBITOS DO DIA".AsSpan());
        meta.Campos[4].Definidor(registro, "N".AsSpan());

        registro.RegCod.Should().Be("I310");
        registro.NumAd.Should().Be(6);
        registro.Campo.Should().Be("VAL_DEB_MF");
        registro.Descricao.Should().Be("TOTAL DOS DEBITOS DO DIA");
        registro.Tipo.Should().Be(TipoDadoCampoAdicional.Numerico);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I020".AsSpan(), out var meta);
        var registro = (RegistroI020)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.Descricao.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|I020|I310|6|VAL_DEB_MF|TOTAL DOS DEBITOS DO DIA|N|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComDescricaoVazia_PreservaTextoCanonico()
    {
        const string sped = "|I020|I155|10|VL_SLD_INI_MF||N|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_TipoCara_PreservaTextoCanonico()
    {
        const string sped = "|I020|I200|7|COD_MOEDA|Código da moeda funcional|C|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("N", TipoDadoCampoAdicional.Numerico)]
    [InlineData("C", TipoDadoCampoAdicional.Caractere)]
    public void TipoDadoCampoAdicional_ConversaoEnum_MapeiaCodigo(string codigo, TipoDadoCampoAdicional esperado)
    {
        _catalogo.TentarObter("I020".AsSpan(), out var meta);
        var registro = (RegistroI020)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, codigo.AsSpan());

        registro.Tipo.Should().Be(esperado);
    }
}
