using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC400Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC400).Assembly);

    [Fact]
    public void Atributo_DeclaraC400_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC400).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C400");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC400ComQuatroCamposNaOrdem()
    {
        _catalogo.TentarObter("C400".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C400");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CodMod", "EcfMod", "EcfFab", "EcfCx"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();    // CodMod
        meta.Campos[1].Tamanho.Should().Be(20);
        meta.Campos[1].Obrigatorio.Should().BeTrue();    // EcfMod
        meta.Campos[2].Tamanho.Should().Be(21);
        meta.Campos[2].Obrigatorio.Should().BeTrue();    // EcfFab
        meta.Campos[3].Tamanho.Should().Be(3);
        meta.Campos[3].Obrigatorio.Should().BeTrue();    // EcfCx
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C400".AsSpan(), out var meta);
        var registro = (RegistroC400)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "02".AsSpan());             // CodMod
        meta.Campos[1].Definidor(registro, "TOSHIBA-T8200".AsSpan()); // EcfMod
        meta.Campos[2].Definidor(registro, "FAB-S001".AsSpan());      // EcfFab
        meta.Campos[3].Definidor(registro, "1".AsSpan());              // EcfCx

        registro.CodMod.Should().Be("02");
        registro.EcfMod.Should().Be("TOSHIBA-T8200");
        registro.EcfFab.Should().Be("FAB-S001");
        registro.EcfCx.Should().Be(1);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C400|02|TOSHIBA-T8200|FAB-S001|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCodMod2D_PreservaTextoCanonico()
    {
        const string sped = "|C400|2D|ECF-MODELO-A|FAB-S002|2|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
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
