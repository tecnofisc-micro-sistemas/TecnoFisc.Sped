using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoD;

public sealed class RegistroD605Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD605).Assembly);

    [Fact]
    public void Atributo_DeclaraD605_Nivel4_BlocoD()
    {
        var atributo = typeof(RegistroD605).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D605");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD605ComOitoCamposNaOrdem()
    {
        _catalogo.TentarObter("D605".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D605");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CodClass", "VlItem", "VlDesc", "CstCofins", "VlBcCofins", "AliqCofins", "VlCofins", "CodCta"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D605".AsSpan(), out var meta);
        var registro = (RegistroD605)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0101".AsSpan());       // CodClass
        meta.Campos[1].Definidor(registro, "5000,00".AsSpan());    // VlItem
        meta.Campos[2].Definidor(registro, "200,00".AsSpan());     // VlDesc
        meta.Campos[3].Definidor(registro, "01".AsSpan());         // CstCofins
        meta.Campos[4].Definidor(registro, "4800,00".AsSpan());    // VlBcCofins
        meta.Campos[5].Definidor(registro, "3,0000".AsSpan());     // AliqCofins
        meta.Campos[6].Definidor(registro, "144,00".AsSpan());     // VlCofins
        meta.Campos[7].Definidor(registro, "3.1.01.003".AsSpan()); // CodCta

        registro.CodClass.Should().Be("0101");
        registro.VlItem.Should().Be(5000.00m);
        registro.VlDesc.Should().Be(200.00m);
        registro.CstCofins.Should().Be("01");
        registro.VlBcCofins.Should().Be(4800.00m);
        registro.AliqCofins.Should().Be(3.00m);
        registro.VlCofins.Should().Be(144.00m);
        registro.CodCta.Should().Be("3.1.01.003");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D605".AsSpan(), out var meta);
        var registro = (RegistroD605)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlDesc
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlBcCofins
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodCta

        registro.VlDesc.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|D605|0101|5000,00|200,00|01|4800,00|3,0000|144,00|3.1.01.003|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemBaseCalculoOpcional_PreservaTextoCanonico()
    {
        const string sped = "|D605|0201|10000,00||49|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

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
}
