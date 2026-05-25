using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.041 — exercita a forma do <see cref="RegistroI510"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 162–163): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI510Tests
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
    public void Atributo_DeclaraI510_Nivel3_BlocoI()
    {
        var atributo = typeof(RegistroI510).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I510");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI510Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("I510".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I510");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["NmCampo", "DescCampo", "TipoCampo", "TamCampo", "DecCampo", "ColCampo"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I510".AsSpan(), out var meta);
        var registro = (RegistroI510)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "COD_PROD".AsSpan());         // NmCampo
        meta.Campos[1].Definidor(registro, "CODIGO_DO_PRODUTO".AsSpan()); // DescCampo
        meta.Campos[2].Definidor(registro, "C".AsSpan());                 // TipoCampo
        meta.Campos[3].Definidor(registro, "13".AsSpan());                // TamCampo
        meta.Campos[4].Definidor(registro, "2".AsSpan());                 // DecCampo
        meta.Campos[5].Definidor(registro, "15".AsSpan());                // ColCampo

        registro.NmCampo.Should().Be("COD_PROD");
        registro.DescCampo.Should().Be("CODIGO_DO_PRODUTO");
        registro.TipoCampo.Should().Be(TipoCampoRazaoAuxiliar.Caractere);
        registro.TamCampo.Should().Be(13);
        registro.DecCampo.Should().Be(2);
        registro.ColCampo.Should().Be(15);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I510".AsSpan(), out var meta);
        var registro = (RegistroI510)meta!.Fabrica();

        // DecCampo (campo 06) é opcional — span vazio → null
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.DecCampo.Should().BeNull();
    }

    [Theory]
    [InlineData("N", TipoCampoRazaoAuxiliar.Numerico)]
    [InlineData("C", TipoCampoRazaoAuxiliar.Caractere)]
    public void Definidor_TipoCampo_MapeiaValoresValidos(string valor, TipoCampoRazaoAuxiliar esperado)
    {
        _catalogo.TentarObter("I510".AsSpan(), out var meta);
        var registro = (RegistroI510)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, valor.AsSpan());

        registro.TipoCampo.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual: campo caractere sem decimais
        const string sped = "|I510|COD_PROD|CÓDIGO_DO_PRODUTO|C|13||15|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CampoNumericoComDecimais_PreservaTextoCanonico()
    {
        // Campo numérico com casas decimais preenchidas
        const string sped = "|I510|VL_BRUTO|Valor Bruto|N|15|2|20|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
