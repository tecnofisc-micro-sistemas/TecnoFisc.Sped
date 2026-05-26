using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoJ;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoJ;

/// <summary>
/// Sub-stage 10.051 — exercita a forma do <see cref="RegistroJ800"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 190–192): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroJ800Tests
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
    public void Atributo_DeclaraJ800_Nivel3_BlocoJ()
    {
        var atributo = typeof(RegistroJ800).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("J800");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("J");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroJ800Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("J800".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("J800");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "TipoDoc", "DescRtf", "HashRtf", "ArqRtf", "IndFimRtf",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("J800".AsSpan(), out var meta);
        var registro = (RegistroJ800)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "001".AsSpan());                                              // TipoDoc
        meta.Campos[1].Definidor(registro, "Notas Explicativas".AsSpan());                               // DescRtf
        meta.Campos[2].Definidor(registro, "1234567890ABCDEFABCDEFABCDEFAB12345678900".AsSpan());        // HashRtf
        meta.Campos[3].Definidor(registro, @"{\rtf1\ansi\ansicpg1252\uc1 conteudo}".AsSpan());           // ArqRtf
        meta.Campos[4].Definidor(registro, "J800FIM".AsSpan());                                          // IndFimRtf

        registro.TipoDoc.Should().Be(TipoDocumentoJ800.DemonstracaoResultadoAbrangente);
        registro.DescRtf.Should().Be("Notas Explicativas");
        registro.HashRtf.Should().Be("1234567890ABCDEFABCDEFABCDEFAB12345678900");
        registro.ArqRtf.Should().Be(@"{\rtf1\ansi\ansicpg1252\uc1 conteudo}");
        registro.IndFimRtf.Should().Be("J800FIM");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("J800".AsSpan(), out var meta);
        var registro = (RegistroJ800)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty); // DescRtf
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // HashRtf

        registro.DescRtf.Should().BeNull();
        registro.HashRtf.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_NotasExplicativas_PreservaTextoCanonico()
    {
        // Exemplo do manual p. 192 — notas explicativas, hash 41 chars, RTF simplificado
        const string sped = @"|J800|010|Notas Explicativas|1234567890ABCDEFABCDEFABCDEFAB12345678900|{\rtf1\ansi conteudo}|J800FIM|" + "\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Campos opcionais DESC_RTF e HASH_RTF ausentes
        const string sped = @"|J800|002|||{\rtf1 fluxo de caixa}|J800FIM|" + "\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
