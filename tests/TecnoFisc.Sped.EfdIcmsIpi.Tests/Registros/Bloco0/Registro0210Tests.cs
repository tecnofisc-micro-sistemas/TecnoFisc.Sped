using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.013 — exercita a forma do <see cref="Registro0210"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 37): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// Sub-stage 8.016.005 — verifica descontinuação em V016: <c>[Descontinuado]</c> aplicado
/// como informacional. Parser continua aceitando o registro em V016+ porque arquivos
/// históricos ainda contêm <c>0210</c> e devem ser lidos (ARCHITECTURE §4.7 read-only).
/// </summary>
public sealed class Registro0210Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0210).Assembly);

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }

    [Fact]
    public void Atributo_Declara0210_Nivel3_Bloco0()
    {
        var atributo = typeof(Registro0210).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0210");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0210Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("0210".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0210");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodItemComp",
            "QtdComp",
            "Perda",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0210".AsSpan(), out var meta);
        var registro = (Registro0210)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "MAT-001".AsSpan());
        meta.Campos[1].Definidor(registro, "2.500000".AsSpan());
        meta.Campos[2].Definidor(registro, "1.2500".AsSpan());

        registro.CodItemComp.Should().Be("MAT-001");
        registro.QtdComp.Should().Be(2.5m);
        registro.Perda.Should().Be(1.25m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0210".AsSpan(), out var meta);
        var registro = (Registro0210)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.CodItemComp.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0210|MAT-001|2,500000|1,2500|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComQuantidadeInteira_PreservaTextoCanonico()
    {
        const string sped = "|0210|INSUMO-QUIMICO-A|1,000000|0,5000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    // ── sub-stage 8.016.005: descontinuação em V016 ──────────────────────────

    [Fact]
    public void Atributo_Descontinuado_DeclaraEmVersaoV016()
    {
        var atributo = typeof(Registro0210).GetCustomAttribute<DescontinuadoAttribute>();

        atributo.Should().NotBeNull();
        atributo!.EmVersao.Should().Be((int)LayoutEfdIcmsIpi.V016);
    }

    [Fact]
    public void Catalogo_Registro0210_ExpoeDescontinuadoEm16()
    {
        _catalogo.TentarObter("0210".AsSpan(), out var meta).Should().BeTrue();
        meta!.DescontinuadoEm.Should().Be((int)LayoutEfdIcmsIpi.V016);
    }

    [Fact]
    public async Task Parser_Registro0210_EmArquivoV015_Aceito()
    {
        // V015 → ainda válido; 0210 não estava descontinuado
        const string sped =
            "|0000|015|0|01012022|31012022|EMPRESA TESTE SA|11222333000181||SP|123456789|3550308|||A|0|\r\n" +
            "|0210|MAT-001|2,500000|1,2500|\r\n" +
            "|9999|2|\r\n";

        var leitor = new LeitorSpedTxt(_catalogo);
        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();

        Func<Task> ler = async () =>
        {
            await foreach (var r in leitor.LerStreamingAsync(entrada, TestContext.Current.CancellationToken))
                registros.Add(r);
        };

        await ler.Should().NotThrowAsync();
        registros.Should().Contain(r => r.Codigo == "0210");
    }

    [Fact]
    public async Task Parser_Registro0210_EmArquivoV016_AceitoComoInformacional()
    {
        // V016 → descontinuado, mas leitura mantém-se ativa para arquivos históricos.
        // Anotação [Descontinuado] é informacional no read path (ARCHITECTURE §4.7).
        const string sped =
            "|0000|016|0|01012022|31012022|EMPRESA TESTE SA|11222333000181||SP|123456789|3550308|||A|0|\r\n" +
            "|0210|MAT-001|2,500000|1,2500|\r\n" +
            "|9999|2|\r\n";

        var leitor = new LeitorSpedTxt(_catalogo);
        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();

        Func<Task> ler = async () =>
        {
            await foreach (var r in leitor.LerStreamingAsync(entrada, TestContext.Current.CancellationToken))
                registros.Add(r);
        };

        await ler.Should().NotThrowAsync();
        registros.Should().Contain(r => r.Codigo == "0210");
    }
}
