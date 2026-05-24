using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.045 — exercita a forma do <see cref="RegistroC115"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 70): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC115Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC115).Assembly);

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
    public void Atributo_DeclaraC115_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC115).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C115");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC115Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("C115".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C115");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndCarga", "CnpjCol", "IeCol", "CpfCol", "CodMunCol",
            "CnpjEntg", "IeEntg", "CpfEntg", "CodMunEntg",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 9));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C115".AsSpan(), out var meta);
        var registro = (RegistroC115)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());              // IndCarga
        meta.Campos[1].Definidor(registro, "11222333000181".AsSpan()); // CnpjCol
        meta.Campos[2].Definidor(registro, "111222333".AsSpan());      // IeCol
        meta.Campos[3].Definidor(registro, Span<char>.Empty);          // CpfCol (vazio)
        meta.Campos[4].Definidor(registro, "3550308".AsSpan());        // CodMunCol
        meta.Campos[5].Definidor(registro, Span<char>.Empty);          // CnpjEntg (vazio)
        meta.Campos[6].Definidor(registro, Span<char>.Empty);          // IeEntg (vazio)
        meta.Campos[7].Definidor(registro, "52998224725".AsSpan());    // CpfEntg
        meta.Campos[8].Definidor(registro, "3550308".AsSpan());        // CodMunEntg

        registro.IndCarga.Should().Be(IndicadorTipoCarga.Rodoviario);
        registro.CnpjCol.Should().Be(Cnpj.Create("11222333000181"));
        registro.IeCol.Should().Be(InscricaoEstadual.Create("111222333"));
        registro.CpfCol.Should().BeNull();
        registro.CodMunCol.Should().Be("3550308");
        registro.CnpjEntg.Should().BeNull();
        registro.IeEntg.Should().BeNull();
        registro.CpfEntg.Should().Be(Cpf.Create("52998224725"));
        registro.CodMunEntg.Should().Be("3550308");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C115".AsSpan(), out var meta);
        var registro = (RegistroC115)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // IndCarga
        meta.Campos[1].Definidor(registro, Span<char>.Empty);  // CnpjCol
        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // IeCol
        meta.Campos[3].Definidor(registro, Span<char>.Empty);  // CpfCol
        meta.Campos[4].Definidor(registro, Span<char>.Empty);  // CodMunCol
        meta.Campos[5].Definidor(registro, Span<char>.Empty);  // CnpjEntg
        meta.Campos[6].Definidor(registro, Span<char>.Empty);  // IeEntg
        meta.Campos[7].Definidor(registro, Span<char>.Empty);  // CpfEntg
        meta.Campos[8].Definidor(registro, Span<char>.Empty);  // CodMunEntg

        registro.IndCarga.Should().BeNull();
        registro.CnpjCol.Should().BeNull();
        registro.IeCol.Should().BeNull();
        registro.CpfCol.Should().BeNull();
        registro.CodMunCol.Should().BeNull();
        registro.CnpjEntg.Should().BeNull();
        registro.IeEntg.Should().BeNull();
        registro.CpfEntg.Should().BeNull();
        registro.CodMunEntg.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Transporte rodoviário: coleta com CNPJ e IE, entrega com CPF.
        const string sped = "|C115|0|11222333000181|111222333||3550308|||52998224725|3550308|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Somente IND_CARGA preenchido (aéreo), todos os campos de coleta/entrega ausentes.
        const string sped = "|C115|5|||||||||\r\n";

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
    public void Definidor_IndCargaCobertosEnum(string valor, IndicadorTipoCarga esperado)
    {
        _catalogo.TentarObter("C115".AsSpan(), out var meta);
        var registro = (RegistroC115)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, valor.AsSpan());

        registro.IndCarga.Should().Be(esperado);
    }
}
