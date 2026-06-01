using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.058 — exercita a forma do <see cref="RegistroC175"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 84): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC175Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC175).Assembly);

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
    public void Atributo_DeclaraC175_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC175).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C175");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC175Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("C175".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C175");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndVeicOper", "Cnpj", "Uf", "ChassiVeic",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 4));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C175".AsSpan(), out var meta);
        var registro = (RegistroC175)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());                    // IndVeicOper
        meta.Campos[1].Definidor(registro, "11222333000181".AsSpan());       // Cnpj
        meta.Campos[2].Definidor(registro, "SP".AsSpan());                   // Uf
        meta.Campos[3].Definidor(registro, "9BWZZZ377VT004251".AsSpan());    // ChassiVeic

        registro.IndVeicOper.Should().Be(IndicadorOperacaoVeiculo.FaturamentoDireto);
        registro.Cnpj.Should().Be(Cnpj.Create("11222333000181"));
        registro.Uf.Should().Be("SP");
        registro.ChassiVeic.Should().Be("9BWZZZ377VT004251");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C175".AsSpan(), out var meta);
        var registro = (RegistroC175)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.IndVeicOper.Should().BeNull();
        registro.Cnpj.Should().BeNull();
        registro.Uf.Should().BeNull();
        registro.ChassiVeic.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Faturamento direto, concessionária SP, chassi válido.
        const string sped = "|C175|1|11222333000181|SP|9BWZZZ377VT004251|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Venda para concessionária sem CNPJ/UF preenchidos.
        const string sped = "|C175|0|||9BWZZZ377VT004251|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorOperacaoVeiculo.VendaParaConcessionaria)]
    [InlineData("1", IndicadorOperacaoVeiculo.FaturamentoDireto)]
    [InlineData("2", IndicadorOperacaoVeiculo.VendaDireta)]
    [InlineData("3", IndicadorOperacaoVeiculo.VendaDaConcessionaria)]
    [InlineData("9", IndicadorOperacaoVeiculo.Outros)]
    public void Definidor_IndVeicOper_CobertosEnum(string valor, IndicadorOperacaoVeiculo esperado)
    {
        _catalogo.TentarObter("C175".AsSpan(), out var meta);
        var registro = (RegistroC175)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, valor.AsSpan());

        registro.IndVeicOper.Should().Be(esperado);
    }
}
