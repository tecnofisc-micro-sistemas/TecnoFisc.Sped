using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

public sealed class Registro0900Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0900).Assembly);

    [Fact]
    public void Atributo_Declara0900_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0900).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0900");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0900Com14CamposNaOrdem()
    {
        _catalogo.TentarObter("0900".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0900");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "RecTotalBlocoA",
            "RecNrbBlocoA",
            "RecTotalBlocoC",
            "RecNrbBlocoC",
            "RecTotalBlocoD",
            "RecNrbBlocoD",
            "RecTotalBlocoF",
            "RecNrbBlocoF",
            "RecTotalBlocoI",
            "RecNrbBlocoI",
            "RecTotalBloco1",
            "RecNrbBloco1",
            "RecTotalPeriodo",
            "RecTotalNrbPeriodo",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0900".AsSpan(), out var meta);
        var registro = (Registro0900)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1000,00".AsSpan());
        meta.Campos[1].Definidor(registro, "100,00".AsSpan());
        meta.Campos[2].Definidor(registro, "2000,00".AsSpan());
        meta.Campos[3].Definidor(registro, "200,00".AsSpan());
        meta.Campos[4].Definidor(registro, "3000,00".AsSpan());
        meta.Campos[5].Definidor(registro, "300,00".AsSpan());
        meta.Campos[6].Definidor(registro, "4000,00".AsSpan());
        meta.Campos[7].Definidor(registro, "400,00".AsSpan());
        meta.Campos[8].Definidor(registro, "5000,00".AsSpan());
        meta.Campos[9].Definidor(registro, "500,00".AsSpan());
        meta.Campos[10].Definidor(registro, "6000,00".AsSpan());
        meta.Campos[11].Definidor(registro, "600,00".AsSpan());
        meta.Campos[12].Definidor(registro, "21000,00".AsSpan());
        meta.Campos[13].Definidor(registro, "2100,00".AsSpan());

        registro.RecTotalBlocoA.Should().Be(1000.00m);
        registro.RecNrbBlocoA.Should().Be(100.00m);
        registro.RecTotalBlocoC.Should().Be(2000.00m);
        registro.RecNrbBlocoC.Should().Be(200.00m);
        registro.RecTotalBlocoD.Should().Be(3000.00m);
        registro.RecNrbBlocoD.Should().Be(300.00m);
        registro.RecTotalBlocoF.Should().Be(4000.00m);
        registro.RecNrbBlocoF.Should().Be(400.00m);
        registro.RecTotalBlocoI.Should().Be(5000.00m);
        registro.RecNrbBlocoI.Should().Be(500.00m);
        registro.RecTotalBloco1.Should().Be(6000.00m);
        registro.RecNrbBloco1.Should().Be(600.00m);
        registro.RecTotalPeriodo.Should().Be(21000.00m);
        registro.RecTotalNrbPeriodo.Should().Be(2100.00m);
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("0900".AsSpan(), out var meta);
        var registro = (Registro0900)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.RecNrbBlocoA.Should().BeNull();
        registro.RecNrbBlocoC.Should().BeNull();
        registro.RecTotalNrbPeriodo.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0900|1000,00|100,00|2000,00|200,00|3000,00|300,00|4000,00|400,00|5000,00|500,00|6000,00|600,00|21000,00|2100,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|0900|1000,00||2000,00||3000,00||4000,00||5000,00||6000,00||21000,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
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
