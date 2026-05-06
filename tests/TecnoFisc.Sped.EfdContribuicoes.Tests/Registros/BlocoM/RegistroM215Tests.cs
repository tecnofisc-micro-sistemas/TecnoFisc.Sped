using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM215Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM215).Assembly);

    [Fact]
    public void Atributo_DeclaraM215_Nivel4_BlocoM()
    {
        var atributo = typeof(RegistroM215).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M215");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM215Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("M215".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M215");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndAjBc", "VlAjBc", "CodAjBc", "NumDoc", "DescrAjBc",
            "DtRef", "CodCta", "Cnpj", "InfoCompl",
        ]);
        meta.Campos[0].Tamanho.Should().Be(1);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // IndAjBc
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlAjBc
        meta.Campos[2].Tamanho.Should().Be(2);
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // CodAjBc
        meta.Campos[3].Obrigatorio.Should().BeFalse();  // NumDoc
        meta.Campos[4].Obrigatorio.Should().BeFalse();  // DescrAjBc
        meta.Campos[5].Obrigatorio.Should().BeFalse();  // DtRef
        meta.Campos[6].Tamanho.Should().Be(255);
        meta.Campos[7].Tamanho.Should().Be(14);
        meta.Campos[7].Obrigatorio.Should().BeTrue();   // Cnpj
        meta.Campos[8].Obrigatorio.Should().BeFalse();  // InfoCompl
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M215".AsSpan(), out var meta);
        var registro = (RegistroM215)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());                   // IndAjBc
        meta.Campos[1].Definidor(registro, "3000,00".AsSpan());             // VlAjBc
        meta.Campos[2].Definidor(registro, "01".AsSpan());                  // CodAjBc
        meta.Campos[3].Definidor(registro, "PROC-2024-999".AsSpan());       // NumDoc
        meta.Campos[4].Definidor(registro, "Exclusão de receita".AsSpan()); // DescrAjBc
        meta.Campos[5].Definidor(registro, "31012024".AsSpan());            // DtRef
        meta.Campos[6].Definidor(registro, "1.2.3.001".AsSpan());           // CodCta
        meta.Campos[7].Definidor(registro, "11222333000181".AsSpan());      // Cnpj
        meta.Campos[8].Definidor(registro, "Ajuste autorizado".AsSpan());   // InfoCompl

        registro.IndAjBc.Should().Be(IndicadorAjuste.Reducao);
        registro.VlAjBc.Should().Be(3000m);
        registro.CodAjBc.Should().Be("01");
        registro.NumDoc.Should().Be("PROC-2024-999");
        registro.DescrAjBc.Should().Be("Exclusão de receita");
        registro.DtRef.Should().Be(new DateOnly(2024, 1, 31));
        registro.CodCta.Should().Be("1.2.3.001");
        registro.Cnpj.Should().Be(Cnpj.Criar("11222333000181"));
        registro.InfoCompl.Should().Be("Ajuste autorizado");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M215".AsSpan(), out var meta);
        var registro = (RegistroM215)meta!.Fabrica();

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
            "|M215|0|3000,00|01|PROC-2024-999|Exclusão de receita|31012024|1.2.3.001|11222333000181|Ajuste autorizado|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Ajuste de acréscimo sem processo, descrição, data e informação complementar
        const string sped = "|M215|1|1500,00|02|||||11222333000181||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorAjuste.Reducao)]
    [InlineData("1", IndicadorAjuste.Acrescimo)]
    public void IndicadorAjuste_Roundtrip_MapeiaCodigo(string codigo, IndicadorAjuste esperado)
    {
        _catalogo.TentarObter("M215".AsSpan(), out var meta);
        var registro = (RegistroM215)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, codigo.AsSpan());

        registro.IndAjBc.Should().Be(esperado);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
