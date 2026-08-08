using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.235 — exercita a forma do <see cref="Registro1600"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 286): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// Sub-stage 8.016.004 — verifica descontinuação em V016: <c>[Descontinuado]</c> aplicado
/// como informacional. Parser continua aceitando o registro em V016+ porque arquivos
/// históricos ainda contêm <c>1600</c> e devem ser lidos (ARCHITECTURE §4.7 read-only).
/// </summary>
public sealed class Registro1600Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1600).Assembly);

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
    public void Atributo_Declara1600_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1600).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1600");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1600Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("1600".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1600");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodPart", "TotCredito", "TotDebito"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 3));
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "CodPart", "TotCredito", "TotDebito"
        ]);
        meta.Campos[0].Tamanho.Should().Be(60);
        meta.Campos[1].Decimais.Should().Be(2);
        meta.Campos[2].Decimais.Should().Be(2);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1600".AsSpan(), out var meta);
        var registro = (Registro1600)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "BANCO001".AsSpan());
        meta.Campos[1].Definidor(registro, "12345,67".AsSpan());
        meta.Campos[2].Definidor(registro, "7654,32".AsSpan());

        registro.CodPart.Should().Be("BANCO001");
        registro.TotCredito.Should().Be(12345.67m);
        registro.TotDebito.Should().Be(7654.32m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1600".AsSpan(), out var meta);
        var registro = (Registro1600)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodPart.Should().BeNull();
        registro.TotCredito.Should().Be(0m);
        registro.TotDebito.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1600|BANCO001|12345,67|7654,32|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresZerados_PreservaTextoCanonico()
    {
        const string sped = "|1600|PAGTO001|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    // ── sub-stage 8.016.004: descontinuação em V016 ──────────────────────────

    [Fact]
    public void Atributo_Descontinuado_DeclaraEmVersaoV016()
    {
        var atributo = typeof(Registro1600).GetCustomAttribute<DescontinuadoAttribute>();

        atributo.Should().NotBeNull();
        atributo!.EmVersao.Should().Be((int)LayoutEfdIcmsIpi.V016);
    }

    [Fact]
    public void Catalogo_Registro1600_ExpoeDescontinuadoEm16()
    {
        _catalogo.TentarObter("1600".AsSpan(), out var meta).Should().BeTrue();
        meta!.DescontinuadoEm.Should().Be((int)LayoutEfdIcmsIpi.V016);
    }

    /// <summary>
    /// Mesma asserção de <see cref="Catalogo_Registro1600_ExpoeDescontinuadoEm16"/>, mas contra o
    /// catálogo gerado em compile-time (<c>CatalogoSpedGerado</c>) — o que <c>ParserEfdIcmsIpi</c>
    /// realmente usa em produção, e não o catálogo reflexivo (<see cref="_catalogo"/>) usado só em
    /// teste. Cobre a lacuna do <c>RegistroSpedCatalogoGenerator</c> que deixava
    /// <c>DescontinuadoEm</c> zerado no catálogo gerado (PR 531, achado de follow-up).
    /// </summary>
    [Fact]
    public void CatalogoGerado_Registro1600_ExpoeDescontinuadoEm16()
    {
        new CatalogoSpedGerado().TentarObter("1600".AsSpan(), out var meta).Should().BeTrue();
        meta!.DescontinuadoEm.Should().Be((int)LayoutEfdIcmsIpi.V016);
    }

    [Fact]
    public async Task Parser_Registro1600_EmArquivoV015_Aceito()
    {
        // V015 → ainda válido; 1600 não estava descontinuado
        const string sped =
            "|0000|015|0|01012022|31012022|EMPRESA TESTE SA|11222333000181||SP|123456789|3550308|||A|0|\r\n" +
            "|1600|BANCO001|1000,00|500,00|\r\n" +
            "|9999|2|\r\n";

        var leitor = new LeitorSpedTxt(_catalogo);
        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();

        Func<Task> ler = async () =>
        {
            await foreach (var r in leitor.ReadStreamingAsync(entrada, TestContext.Current.CancellationToken))
                registros.Add(r);
        };

        await ler.Should().NotThrowAsync();
        registros.Should().Contain(r => r.Codigo == "1600");
    }

    [Fact]
    public async Task Parser_Registro1600_EmArquivoV016_AceitoComoInformacional()
    {
        // V016 → descontinuado, mas leitura mantém-se ativa para arquivos históricos.
        // Anotação [Descontinuado] é informacional no read path (ARCHITECTURE §4.7).
        const string sped =
            "|0000|016|0|01012022|31012022|EMPRESA TESTE SA|11222333000181||SP|123456789|3550308|||A|0|\r\n" +
            "|1600|BANCO001|1000,00|500,00|\r\n" +
            "|9999|2|\r\n";

        var leitor = new LeitorSpedTxt(_catalogo);
        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();

        Func<Task> ler = async () =>
        {
            await foreach (var r in leitor.ReadStreamingAsync(entrada, TestContext.Current.CancellationToken))
                registros.Add(r);
        };

        await ler.Should().NotThrowAsync();
        registros.Should().Contain(r => r.Codigo == "1600");
    }
}
