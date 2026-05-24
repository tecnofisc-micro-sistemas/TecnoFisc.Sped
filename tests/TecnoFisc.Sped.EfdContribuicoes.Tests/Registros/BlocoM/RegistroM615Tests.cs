using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM615Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM615).Assembly);

    [Fact]
    public void Atributo_DeclaraM615_Nivel4_BlocoM()
    {
        var atributo = typeof(RegistroM615).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M615");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM615Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("M615".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M615");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndAjBc", "VlAjBc", "CodAjBc", "NumDoc", "DescrAjBc", "DtRef", "CodCta", "Cnpj", "InfoCompl",
        ]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // IndAjBc
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlAjBc
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // CodAjBc
        meta.Campos[3].Obrigatorio.Should().BeFalse();  // NumDoc
        meta.Campos[7].Tamanho.Should().Be(14);
        meta.Campos[7].Obrigatorio.Should().BeTrue();   // Cnpj
        meta.Campos[8].Obrigatorio.Should().BeFalse();  // InfoCompl
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M615".AsSpan(), out var meta);
        var registro = (RegistroM615)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());                // IndAjBc
        meta.Campos[1].Definidor(registro, "2000,00".AsSpan());          // VlAjBc
        meta.Campos[2].Definidor(registro, "01".AsSpan());               // CodAjBc
        meta.Campos[3].Definidor(registro, "PROC-2024-001".AsSpan());    // NumDoc
        meta.Campos[4].Definidor(registro, "Ajuste base Cofins".AsSpan()); // DescrAjBc
        meta.Campos[5].Definidor(registro, "31012024".AsSpan());         // DtRef
        meta.Campos[6].Definidor(registro, "1.1.01.001".AsSpan());       // CodCta
        meta.Campos[7].Definidor(registro, "11222333000181".AsSpan());   // Cnpj
        meta.Campos[8].Definidor(registro, "Info adicional".AsSpan());   // InfoCompl

        registro.IndAjBc.Should().Be(IndicadorAjuste.Acrescimo);
        registro.VlAjBc.Should().Be(2000m);
        registro.CodAjBc.Should().Be("01");
        registro.NumDoc.Should().Be("PROC-2024-001");
        registro.DtRef.Should().Be(new DateOnly(2024, 1, 31));
        registro.Cnpj.Should().Be(Cnpj.Create("11222333000181"));
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M615".AsSpan(), out var meta);
        var registro = (RegistroM615)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // NumDoc
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // DescrAjBc
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // DtRef
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty); // InfoCompl

        registro.NumDoc.Should().BeNull();
        registro.DescrAjBc.Should().BeNull();
        registro.DtRef.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.InfoCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|M615|1|2000,00|01|PROC-2024-001|Ajuste base Cofins|31012024|1.1.01.001|11222333000181|Info adicional|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|M615|0|500,00|02|||||11222333000181||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorAjuste.Reducao)]
    [InlineData("1", IndicadorAjuste.Acrescimo)]
    public void IndicadorAjuste_Roundtrip_MapeiaCodigo(string codigo, IndicadorAjuste esperado)
    {
        _catalogo.TentarObter("M615".AsSpan(), out var meta);
        var registro = (RegistroM615)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, codigo.AsSpan());

        registro.IndAjBc.Should().Be(esperado);
    }

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
}
