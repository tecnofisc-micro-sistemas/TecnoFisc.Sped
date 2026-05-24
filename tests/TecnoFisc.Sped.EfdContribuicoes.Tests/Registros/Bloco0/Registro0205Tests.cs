using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

public sealed class Registro0205Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0205).Assembly);

    [Fact]
    public void Atributo_Declara0205_Nivel4_Bloco0()
    {
        var atributo = typeof(Registro0205).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0205");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0205Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("0205".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0205");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "DescrAntItem",
            "DtIni",
            "DtFim",
            "CodAntItem",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0205".AsSpan(), out var meta);
        var registro = (Registro0205)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "Descricao Anterior do Item".AsSpan());
        meta.Campos[1].Definidor(registro, "01012020".AsSpan());
        meta.Campos[2].Definidor(registro, "31122020".AsSpan());
        meta.Campos[3].Definidor(registro, "COD-ANT-001".AsSpan());

        registro.DescrAntItem.Should().Be("Descricao Anterior do Item");
        registro.DtIni.Should().Be(new DateOnly(2020, 1, 1));
        registro.DtFim.Should().Be(new DateOnly(2020, 12, 31));
        registro.CodAntItem.Should().Be("COD-ANT-001");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("0205".AsSpan(), out var meta);
        var registro = (Registro0205)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.DescrAntItem.Should().BeNull();
        registro.CodAntItem.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0205|Descricao Anterior|01012020|31122020|COD-ANT|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|0205||01012021|31122021||\r\n";

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
