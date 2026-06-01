using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.039 — exercita a forma do <see cref="RegistroC105"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 66): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC105Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC105).Assembly);

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
    public void Atributo_DeclaraC105_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC105).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C105");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC105Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("C105".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C105");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "Oper", "Uf",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 2));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C105".AsSpan(), out var meta);
        var registro = (RegistroC105)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());   // Oper
        meta.Campos[1].Definidor(registro, "SP".AsSpan());  // Uf

        registro.Oper.Should().Be(IndicadorOperacaoIcmsSt.LeasingOuFaturamentoDireto);
        registro.Uf.Should().Be("SP");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C105".AsSpan(), out var meta);
        var registro = (RegistroC105)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, Span<char>.Empty);  // Uf (string nullable)

        registro.Uf.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Combustíveis e Lubrificantes com destino SP.
        const string sped = "|C105|0|SP|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComLeasingVeiculos_PreservaTextoCanonico()
    {
        // Leasing de veículos ou faturamento direto com destino RJ.
        const string sped = "|C105|1|RJ|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorOperacaoIcmsSt.CombustiveisLubrificantes)]
    [InlineData("1", IndicadorOperacaoIcmsSt.LeasingOuFaturamentoDireto)]
    public void Definidor_OperCobertosEnum(string valor, IndicadorOperacaoIcmsSt esperado)
    {
        _catalogo.TentarObter("C105".AsSpan(), out var meta);
        var registro = (RegistroC105)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, valor.AsSpan());

        registro.Oper.Should().Be(esperado);
    }
}
