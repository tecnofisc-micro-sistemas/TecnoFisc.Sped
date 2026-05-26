using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.037 — exercita a forma do <see cref="RegistroI310"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 153–155): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI310Tests
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
    public void Atributo_DeclaraI310_Nivel4_BlocoI()
    {
        var atributo = typeof(RegistroI310).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I310");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI310Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("I310".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I310");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodCta", "CodCcus", "ValDebd", "ValCredd"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I310".AsSpan(), out var meta);
        var registro = (RegistroI310)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1.1".AsSpan());        // CodCta
        meta.Campos[1].Definidor(registro, "CC001".AsSpan());      // CodCcus
        meta.Campos[2].Definidor(registro, "50000,00".AsSpan());   // ValDebd
        meta.Campos[3].Definidor(registro, "10000,00".AsSpan());   // ValCredd

        registro.CodCta.Should().Be("1.1");
        registro.CodCcus.Should().Be("CC001");
        registro.ValDebd.Should().Be(50000m);
        registro.ValCredd.Should().Be(10000m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I310".AsSpan(), out var meta);
        var registro = (RegistroI310)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCcus opcional

        registro.CodCcus.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 155): COD_CTA=1.1, sem COD_CCUS, VAL_DEBD=50000,00, VAL_CREDD=10000,00
        const string sped = "|I310|1.1||50000,00|10000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCentroCustos_PreservaTextoCanonico()
    {
        // Conta com centro de custos preenchido
        const string sped = "|I310|2.1.01|CC002|15000,00|15000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposMoedaFuncional_ParseaCamposPadrao()
    {
        // Arquivo com IDENT_MF="S": I310 recebe dois campos adicionais (VAL_DEB_MF, VAL_CRED_MF) via I020.
        // O parser descarta os campos adicionais (não fazem parte do leiaute fixo).
        // O round-trip produz apenas os 5 campos padrão.
        const string entrada = "|I310|1.1||50000,00|10000,00|50000,00|10000,00|\r\n";
        const string esperado = "|I310|1.1||50000,00|10000,00|\r\n";

        var leitor = new LeitorSpedTxt(_catalogo);
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(entrada));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(stream, TestContext.Current.CancellationToken))
            registros.Add(registro);

        var i310 = registros.OfType<RegistroI310>().Single();
        i310.CodCta.Should().Be("1.1");
        i310.CodCcus.Should().BeNull();
        i310.ValDebd.Should().Be(50000m);
        i310.ValCredd.Should().Be(10000m);

        var escritor = new EscritorSpedTxt(_catalogo);
        using var saida = new MemoryStream();
        await escritor.WriteAsync(saida, registros, TestContext.Current.CancellationToken);
        EncodingSped.Latin1.GetString(saida.ToArray()).Should().Be(esperado);
    }
}
