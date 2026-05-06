using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco1;

public sealed class Registro1502Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1502).Assembly);

    [Fact]
    public void Atributo_Declara1502_Nivel4_Bloco1()
    {
        var atributo = typeof(Registro1502).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1502");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1502Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("1502".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1502");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "VlCredCofinsTribMi", "VlCredCofinsNtMi", "VlCredCofinsExp",
        ]);
        meta.Campos[0].Obrigatorio.Should().BeFalse(); // VlCredCofinsTribMi
        meta.Campos[1].Obrigatorio.Should().BeFalse(); // VlCredCofinsNtMi
        meta.Campos[2].Obrigatorio.Should().BeFalse(); // VlCredCofinsExp
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1502".AsSpan(), out var meta);
        var registro = (Registro1502)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1000,00".AsSpan()); // VlCredCofinsTribMi
        meta.Campos[1].Definidor(registro, "500,00".AsSpan());  // VlCredCofinsNtMi
        meta.Campos[2].Definidor(registro, "250,00".AsSpan());  // VlCredCofinsExp

        registro.VlCredCofinsTribMi.Should().Be(1000.00m);
        registro.VlCredCofinsNtMi.Should().Be(500.00m);
        registro.VlCredCofinsExp.Should().Be(250.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1502".AsSpan(), out var meta);
        var registro = (Registro1502)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCredCofinsTribMi
        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCredCofinsNtMi
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCredCofinsExp

        registro.VlCredCofinsTribMi.Should().BeNull();
        registro.VlCredCofinsNtMi.Should().BeNull();
        registro.VlCredCofinsExp.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1502|1000,00|500,00|250,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ApenasReceitaTributadaMercadoInterno_PreservaTextoCanonico()
    {
        const string sped = "|1502|800,00|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ApenasExportacao_PreservaTextoCanonico()
    {
        const string sped = "|1502|||300,00|\r\n";

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
