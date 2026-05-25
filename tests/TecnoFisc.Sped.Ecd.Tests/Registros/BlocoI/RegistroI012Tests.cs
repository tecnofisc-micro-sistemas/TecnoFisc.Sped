using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.021 — exercita a forma do <see cref="RegistroI012"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 105–107): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI012Tests
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
    public void Atributo_DeclaraI012_Nivel3_BlocoI()
    {
        var atributo = typeof(RegistroI012).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I012");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI012ComQuatroCamposNaOrdem()
    {
        _catalogo.TentarObter("I012".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I012");
        meta.Campos.Select(c => c.Nome).Should().Equal(["NumOrd", "NatLivr", "Tipo", "CodHashAux"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I012".AsSpan(), out var meta);
        var registro = (RegistroI012)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());
        meta.Campos[1].Definidor(registro, "DIARIO AUXILIAR DA CONTA BANCOS".AsSpan());
        meta.Campos[2].Definidor(registro, "0".AsSpan());
        meta.Campos[3].Definidor(registro, "33AE96E3D1A5EE6969D78BDC56551F91AE9558F8".AsSpan());

        registro.NumOrd.Should().Be(1);
        registro.NatLivr.Should().Be("DIARIO AUXILIAR DA CONTA BANCOS");
        registro.Tipo.Should().Be(TipoEscrituracaoLivroAuxiliar.Digital);
        registro.CodHashAux.Should().Be("33AE96E3D1A5EE6969D78BDC56551F91AE9558F8");
    }

    [Fact]
    public void Definidor_CampoHashAuxVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I012".AsSpan(), out var meta);
        var registro = (RegistroI012)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodHashAux.Should().BeNull();
    }

    [Theory]
    [InlineData(TipoEscrituracaoLivroAuxiliar.Digital, "0")]
    [InlineData(TipoEscrituracaoLivroAuxiliar.Outros, "1")]
    public void Serializar_Tipo_RetornaCodigo(TipoEscrituracaoLivroAuxiliar tipo, string esperado)
    {
        _catalogo.TentarObter("I012".AsSpan(), out var meta);
        var registro = (RegistroI012)meta!.Fabrica();
        registro.Tipo = tipo;

        meta.Campos[2].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|I012|1|DIARIO AUXILIAR DA CONTA BANCOS|0|33AE96E3D1A5EE6969D78BDC56551F91AE9558F8|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemHashAux_PreservaTextoCanonico()
    {
        const string sped = "|I012|1|DIARIO COM RESCRITURAÇÃO RESUMIDA|0||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_TipoOutros_PreservaTextoCanonico()
    {
        const string sped = "|I012|2|LIVRO AUXILIAR PAPEL|1||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
