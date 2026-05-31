using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.123 — exercita a forma do <see cref="RegistroD161"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 174): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD161Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD161).Assembly);

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
    public void Atributo_DeclaraD161_Nivel4_BlocoD()
    {
        var atributo = typeof(RegistroD161).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D161");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD161Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("D161".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D161");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndCarga", "CnpjCpfCol", "IeCol", "CodMunCol",
            "CnpjCpfEntg", "IeEntg", "CodMunEntg",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 7));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D161".AsSpan(), out var meta);
        var registro = (RegistroD161)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());              // IndCarga
        meta.Campos[1].Definidor(registro, "12345678901234".AsSpan()); // CnpjCpfCol
        meta.Campos[2].Definidor(registro, "SP123456789".AsSpan());    // IeCol
        meta.Campos[3].Definidor(registro, "3550308".AsSpan());        // CodMunCol
        meta.Campos[4].Definidor(registro, "98765432100001".AsSpan()); // CnpjCpfEntg
        meta.Campos[5].Definidor(registro, "MG987654321".AsSpan());    // IeEntg
        meta.Campos[6].Definidor(registro, "3304557".AsSpan());        // CodMunEntg

        registro.IndCarga.Should().Be(IndicadorTipoCarga.Rodoviario);
        registro.CnpjCpfCol.Should().Be("12345678901234");
        registro.IeCol.Should().Be("SP123456789");
        registro.CodMunCol.Should().Be(3550308);
        registro.CnpjCpfEntg.Should().Be("98765432100001");
        registro.IeEntg.Should().Be("MG987654321");
        registro.CodMunEntg.Should().Be(3304557);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("D161".AsSpan(), out var meta);
        var registro = (RegistroD161)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // IndCarga
        meta.Campos[1].Definidor(registro, Span<char>.Empty);  // CnpjCpfCol
        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // IeCol
        meta.Campos[4].Definidor(registro, Span<char>.Empty);  // CnpjCpfEntg
        meta.Campos[5].Definidor(registro, Span<char>.Empty);  // IeEntg

        registro.IndCarga.Should().BeNull();
        registro.CnpjCpfCol.Should().BeNull();
        registro.IeCol.Should().BeNull();
        registro.CnpjCpfEntg.Should().BeNull();
        registro.IeEntg.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Transporte rodoviário: coleta SP, entrega RJ, ambos com CNPJ e IE.
        const string sped = "|D161|0|12345678901234|SP123456789|3550308|98765432100001|MG987654321|3304557|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Somente IND_CARGA e municípios preenchidos.
        const string sped = "|D161|3|||3550308|||3304557|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_LocalExterior_PreservaTextoCanonico()
    {
        // Origem e destino no Exterior — código IBGE 9999999.
        const string sped = "|D161|5|||9999999|||9999999|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorTipoCarga.Rodoviario)]
    [InlineData("1", IndicadorTipoCarga.Ferroviario)]
    [InlineData("2", IndicadorTipoCarga.RodoFerroviario)]
    [InlineData("3", IndicadorTipoCarga.Aquaviario)]
    [InlineData("4", IndicadorTipoCarga.Dutoviario)]
    [InlineData("5", IndicadorTipoCarga.Aereo)]
    [InlineData("9", IndicadorTipoCarga.Outros)]
    public void Definidor_IndCarga_CobertosEnum(string valor, IndicadorTipoCarga esperado)
    {
        _catalogo.TentarObter("D161".AsSpan(), out var meta);
        var registro = (RegistroD161)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, valor.AsSpan());

        registro.IndCarga.Should().Be(esperado);
    }
}
