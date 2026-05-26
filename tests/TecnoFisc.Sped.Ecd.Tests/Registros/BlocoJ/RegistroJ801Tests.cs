using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoJ;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoJ;

/// <summary>
/// Sub-stage 10.052 — exercita a forma do <see cref="RegistroJ801"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 192–195): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroJ801Tests
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
    public void Atributo_DeclaraJ801_Nivel3_BlocoJ()
    {
        var atributo = typeof(RegistroJ801).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("J801");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("J");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroJ801Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("J801".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("J801");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "TipoDoc", "DescRtf", "CodMotSubs", "HashRtf", "ArqRtf", "IndFimRtf",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("J801".AsSpan(), out var meta);
        var registro = (RegistroJ801)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "001".AsSpan());                                              // TipoDoc
        meta.Campos[1].Definidor(registro, "Termo de Verificação para Fins Substituição da ECD".AsSpan()); // DescRtf
        meta.Campos[2].Definidor(registro, "001".AsSpan());                                              // CodMotSubs
        meta.Campos[3].Definidor(registro, "1234567890ABCDEFABCDEFABCDEFAB12345678900".AsSpan());        // HashRtf
        meta.Campos[4].Definidor(registro, @"{\rtf1\ansi\ansicpg1252\uc1 conteudo}".AsSpan());           // ArqRtf
        meta.Campos[5].Definidor(registro, "J801FIM".AsSpan());                                          // IndFimRtf

        registro.TipoDoc.Should().Be("001");
        registro.DescRtf.Should().Be("Termo de Verificação para Fins Substituição da ECD");
        registro.CodMotSubs.Should().Be(CodigoMotivoSubstituicaoEcd.MudancaSaldosConta);
        registro.HashRtf.Should().Be("1234567890ABCDEFABCDEFABCDEFAB12345678900");
        registro.ArqRtf.Should().Be(@"{\rtf1\ansi\ansicpg1252\uc1 conteudo}");
        registro.IndFimRtf.Should().Be("J801FIM");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("J801".AsSpan(), out var meta);
        var registro = (RegistroJ801)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty); // DescRtf
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // HashRtf

        registro.DescRtf.Should().BeNull();
        registro.HashRtf.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_TermoVerificacao_PreservaTextoCanonico()
    {
        // Exemplo do manual p. 195 — termo de verificação, motivo 001, hash 41 chars
        const string sped = @"|J801|001|Termo de Verificação para Fins Substituição da ECD|001|1234567890ABCDEFABCDEFABCDEFAB1234567890|{\rtf1\ansi\ansicpg1252\uc1...}|J801FIM|" + "\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Campos opcionais DESC_RTF e HASH_RTF ausentes
        const string sped = @"|J801|001||099||{\rtf1 outros}|J801FIM|" + "\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("001", CodigoMotivoSubstituicaoEcd.MudancaSaldosConta)]
    [InlineData("002", CodigoMotivoSubstituicaoEcd.AlteracaoAssinatura)]
    [InlineData("003", CodigoMotivoSubstituicaoEcd.AlteracaoDemonstracoes)]
    [InlineData("004", CodigoMotivoSubstituicaoEcd.AlteracaoFormaEscrituracao)]
    [InlineData("005", CodigoMotivoSubstituicaoEcd.AlteracaoNumeroLivro)]
    [InlineData("099", CodigoMotivoSubstituicaoEcd.Outros)]
    public void CodigoMotivoSubstituicaoEcd_Mapeamento_Correto(string codigo, CodigoMotivoSubstituicaoEcd esperado)
    {
        _catalogo.TentarObter("J801".AsSpan(), out var meta);
        var registro = (RegistroJ801)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, codigo.AsSpan());

        registro.CodMotSubs.Should().Be(esperado);
    }
}
