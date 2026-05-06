using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoD;

public sealed class RegistroD205Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD205).Assembly);

    [Fact]
    public void Atributo_DeclaraD205_Nivel4_BlocoD()
    {
        var atributo = typeof(RegistroD205).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D205");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD205ComSeisCamposNaOrdem()
    {
        _catalogo.TentarObter("D205".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D205");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CstCofins", "VlItem", "VlBcCofins", "AliqCofins", "VlCofins", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();    // CstCofins
        meta.Campos[1].Obrigatorio.Should().BeTrue();    // VlItem
        meta.Campos[2].Obrigatorio.Should().BeFalse();   // VlBcCofins
        meta.Campos[3].Tamanho.Should().Be(8);
        meta.Campos[3].Obrigatorio.Should().BeFalse();   // AliqCofins
        meta.Campos[4].Obrigatorio.Should().BeFalse();   // VlCofins
        meta.Campos[5].Tamanho.Should().Be(255);
        meta.Campos[5].Obrigatorio.Should().BeFalse();   // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D205".AsSpan(), out var meta);
        var registro = (RegistroD205)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "49".AsSpan());           // CstCofins
        meta.Campos[1].Definidor(registro, "50000,00".AsSpan());     // VlItem
        meta.Campos[2].Definidor(registro, "50000,00".AsSpan());     // VlBcCofins
        meta.Campos[3].Definidor(registro, "7,6000".AsSpan());       // AliqCofins
        meta.Campos[4].Definidor(registro, "3800,00".AsSpan());      // VlCofins
        meta.Campos[5].Definidor(registro, "3.1.01.002".AsSpan());   // CodCta

        registro.CstCofins.Should().Be(49);
        registro.VlItem.Should().Be(50000m);
        registro.VlBcCofins.Should().Be(50000m);
        registro.AliqCofins.Should().Be(7.6m);
        registro.VlCofins.Should().Be(3800m);
        registro.CodCta.Should().Be("3.1.01.002");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("D205".AsSpan(), out var meta);
        var registro = (RegistroD205)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcCofins
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqCofins
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCofins
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCta

        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CST 49 (Outras Operações de Saída) — código de 2 dígitos naturais.
        const string sped = "|D205|49|50000,00|50000,00|7,6000|3800,00|3.1.01.002|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemBaseAliquotaValor_PreservaTextoCanonico()
    {
        // CST 99 (Outras Operações) — sem base/alíquota/valor Cofins.
        const string sped = "|D205|99|5000,00|||||\r\n";

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
