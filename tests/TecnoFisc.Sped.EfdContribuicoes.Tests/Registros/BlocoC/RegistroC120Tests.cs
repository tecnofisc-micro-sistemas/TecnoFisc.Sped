using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC120Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC120).Assembly);

    [Fact]
    public void Atributo_DeclaraC120_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC120).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C120");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC120ComCincoCamposNaOrdem()
    {
        _catalogo.TentarObter("C120".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C120");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CodDocImp", "NumDocImp", "VlPisImp", "VlCofinsImp", "NumAcdraw"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
        meta.Campos[0].Tamanho.Should().Be(1);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[1].Tamanho.Should().Be(15);
        meta.Campos[1].Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C120".AsSpan(), out var meta);
        var registro = (RegistroC120)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());               // CodDocImp
        meta.Campos[1].Definidor(registro, "DI2024123456789".AsSpan()); // NumDocImp
        meta.Campos[2].Definidor(registro, "100,50".AsSpan());          // VlPisImp
        meta.Campos[3].Definidor(registro, "250,75".AsSpan());          // VlCofinsImp
        meta.Campos[4].Definidor(registro, "12345678901234".AsSpan());  // NumAcdraw

        registro.CodDocImp.Should().Be(CodigoDocumentoImportacao.DeclaracaoImportacao);
        registro.NumDocImp.Should().Be("DI2024123456789");
        registro.VlPisImp.Should().Be(100.50m);
        registro.VlCofinsImp.Should().Be(250.75m);
        registro.NumAcdraw.Should().Be("12345678901234");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C120".AsSpan(), out var meta);
        var registro = (RegistroC120)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // VlPisImp
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofinsImp
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // NumAcdraw

        registro.VlPisImp.Should().BeNull();
        registro.VlCofinsImp.Should().BeNull();
        registro.NumAcdraw.Should().BeNull();
    }

    [Theory]
    [InlineData(CodigoDocumentoImportacao.DeclaracaoImportacao, "0")]
    [InlineData(CodigoDocumentoImportacao.DeclaracaoSimplificadaImportacao, "1")]
    [InlineData(CodigoDocumentoImportacao.DeclaracaoUnicaImportacao, "2")]
    public void Serializar_CodDocImp_RetornaCodigoSpedCorreto(
        CodigoDocumentoImportacao codigo, string esperado)
    {
        _catalogo.TentarObter("C120".AsSpan(), out var meta);
        var registro = (RegistroC120)meta!.Fabrica();
        registro.CodDocImp = codigo;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C120|0|DI2024123456789|100,50|250,75|12345678901234|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|C120|1|000000001234567||||\r\n";

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
