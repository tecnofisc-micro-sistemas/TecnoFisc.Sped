using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.049 — exercita a forma do <see cref="RegistroC140"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 73-74): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC140Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC140).Assembly);

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
    public void Atributo_DeclaraC140_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC140).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C140");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC140Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("C140".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C140");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndEmit", "IndTit", "DescTit", "NumTit", "QtdParc", "VlTit",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 6));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C140".AsSpan(), out var meta);
        var registro = (RegistroC140)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());               // IndEmit = Terceiros
        meta.Campos[1].Definidor(registro, "00".AsSpan());              // IndTit = Duplicata
        meta.Campos[2].Definidor(registro, "Duplicata NF 001".AsSpan()); // DescTit
        meta.Campos[3].Definidor(registro, "12345".AsSpan());           // NumTit
        meta.Campos[4].Definidor(registro, "03".AsSpan());              // QtdParc
        meta.Campos[5].Definidor(registro, "15000.00".AsSpan());        // VlTit

        registro.IndEmit.Should().Be(IndicadorEmissorDocumento.Terceiros);
        registro.IndTit.Should().Be(IndicadorTipoTitulo.Duplicata);
        registro.DescTit.Should().Be("Duplicata NF 001");
        registro.NumTit.Should().Be("12345");
        registro.QtdParc.Should().Be(3);
        registro.VlTit.Should().Be(15000.00m);
    }

    [Fact]
    public void Definidor_CamposOpcionais_ComSpanVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C140".AsSpan(), out var meta);
        var registro = (RegistroC140)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // DescTit
        meta.Campos[3].Definidor(registro, Span<char>.Empty);  // NumTit

        registro.DescTit.Should().BeNull();
        registro.NumTit.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Emissão própria, Duplicata, com descrição, 3 parcelas.
        const string sped = "|C140|0|00|Duplicata NF 001|12345|3|15000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_TerceirosCheque_PreservaTextoCanonico()
    {
        // Terceiros, Cheque, sem descrição complementar, 1 parcela.
        const string sped = "|C140|1|01||NF-CH-001|1|5000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_OutrosTipoComDescricao_PreservaTextoCanonico()
    {
        // IND_TIT = 99 (Outros); DESC_TIT obrigatório neste caso conforme regra do guia.
        const string sped = "|C140|0|99|Letra de Câmbio|LC-001|2|8000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorEmissorDocumento.EmissaoPropria)]
    [InlineData("1", IndicadorEmissorDocumento.Terceiros)]
    public void Definidor_IndEmit_MapeiaTodosOsValores(string valor, IndicadorEmissorDocumento esperado)
    {
        _catalogo.TentarObter("C140".AsSpan(), out var meta);
        var registro = (RegistroC140)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, valor.AsSpan());

        registro.IndEmit.Should().Be(esperado);
    }

    [Theory]
    [InlineData("00", IndicadorTipoTitulo.Duplicata)]
    [InlineData("01", IndicadorTipoTitulo.Cheque)]
    [InlineData("02", IndicadorTipoTitulo.Promissoria)]
    [InlineData("03", IndicadorTipoTitulo.Recibo)]
    [InlineData("99", IndicadorTipoTitulo.Outros)]
    public void Definidor_IndTit_MapeiaTodosOsValores(string valor, IndicadorTipoTitulo esperado)
    {
        _catalogo.TentarObter("C140".AsSpan(), out var meta);
        var registro = (RegistroC140)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, valor.AsSpan());

        registro.IndTit.Should().Be(esperado);
    }
}
