using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM610Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM610).Assembly);

    [Fact]
    public void Atributo_DeclaraM610_Nivel3_BlocoM()
    {
        var atributo = typeof(RegistroM610).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M610");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM610Com15CamposNaOrdem()
    {
        _catalogo.TentarObter("M610".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M610");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodCont", "VlRecBrt", "VlBcCont", "VlAjusAcresBcCofins", "VlAjusReducBcCofins",
            "VlBcContAjus", "AliqCofins", "QuantBcCofins", "AliqCofinsQuant",
            "VlContApur", "VlAjusAcres", "VlAjusReduc", "VlContDifer", "VlContDiferAnt", "VlContPer",
        ]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();    // CodCont
        meta.Campos[6].Obrigatorio.Should().BeFalse();   // AliqCofins
        meta.Campos[7].Obrigatorio.Should().BeFalse();   // QuantBcCofins
        meta.Campos[8].Obrigatorio.Should().BeFalse();   // AliqCofinsQuant
        meta.Campos[12].Obrigatorio.Should().BeFalse();  // VlContDifer
        meta.Campos[13].Obrigatorio.Should().BeFalse();  // VlContDiferAnt
        meta.Campos[14].Obrigatorio.Should().BeTrue();   // VlContPer
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M610".AsSpan(), out var meta);
        var registro = (RegistroM610)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());          // CodCont
        meta.Campos[1].Definidor(registro, "50000,00".AsSpan());    // VlRecBrt
        meta.Campos[2].Definidor(registro, "50000,00".AsSpan());    // VlBcCont
        meta.Campos[3].Definidor(registro, "0,00".AsSpan());        // VlAjusAcresBcCofins
        meta.Campos[4].Definidor(registro, "0,00".AsSpan());        // VlAjusReducBcCofins
        meta.Campos[5].Definidor(registro, "50000,00".AsSpan());    // VlBcContAjus
        meta.Campos[6].Definidor(registro, "7,6000".AsSpan());      // AliqCofins
        meta.Campos[7].Definidor(registro, "".AsSpan());            // QuantBcCofins
        meta.Campos[8].Definidor(registro, "".AsSpan());            // AliqCofinsQuant
        meta.Campos[9].Definidor(registro, "3800,00".AsSpan());     // VlContApur
        meta.Campos[10].Definidor(registro, "0,00".AsSpan());       // VlAjusAcres
        meta.Campos[11].Definidor(registro, "0,00".AsSpan());       // VlAjusReduc
        meta.Campos[12].Definidor(registro, "".AsSpan());           // VlContDifer
        meta.Campos[13].Definidor(registro, "".AsSpan());           // VlContDiferAnt
        meta.Campos[14].Definidor(registro, "3800,00".AsSpan());    // VlContPer

        registro.CodCont.Should().Be(CodigoContribuicaoSocialApurada.NaoCumulativaAliquotaBasica);
        registro.VlRecBrt.Should().Be(50000m);
        registro.VlBcCont.Should().Be(50000m);
        registro.AliqCofins.Should().Be(7.6m);
        registro.QuantBcCofins.Should().BeNull();
        registro.AliqCofinsQuant.Should().BeNull();
        registro.VlContApur.Should().Be(3800m);
        registro.VlContDifer.Should().BeNull();
        registro.VlContDiferAnt.Should().BeNull();
        registro.VlContPer.Should().Be(3800m);
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M610".AsSpan(), out var meta);
        var registro = (RegistroM610)meta!.Fabrica();

        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqCofins
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // QuantBcCofins
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqCofinsQuant
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty); // VlContDifer
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty); // VlContDiferAnt

        registro.AliqCofins.Should().BeNull();
        registro.QuantBcCofins.Should().BeNull();
        registro.AliqCofinsQuant.Should().BeNull();
        registro.VlContDifer.Should().BeNull();
        registro.VlContDiferAnt.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Contribuição não cumulativa alíquota básica (COD_CONT=01), com alíquota percentual
        const string sped =
            "|M610|01|50000,00|50000,00|0,00|0,00|50000,00|7,6000|||3800,00|0,00|0,00|||3800,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ContribuicaoCumulativa_PreservaTextoCanonico()
    {
        // Contribuição cumulativa alíquota básica (COD_CONT=51)
        const string sped =
            "|M610|51|80000,00|80000,00|0,00|0,00|80000,00|3,0000|||2400,00|0,00|0,00|||2400,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("01", CodigoContribuicaoSocialApurada.NaoCumulativaAliquotaBasica)]
    [InlineData("02", CodigoContribuicaoSocialApurada.NaoCumulativaAliquotasDiferenciadas)]
    [InlineData("03", CodigoContribuicaoSocialApurada.NaoCumulativaAliquotaUnidadeMedida)]
    [InlineData("04", CodigoContribuicaoSocialApurada.NaoCumulativaAliquotaBasicaAtividadeImobiliaria)]
    [InlineData("31", CodigoContribuicaoSocialApurada.SubstituicaoTributaria)]
    [InlineData("32", CodigoContribuicaoSocialApurada.SubstituicaoTributariaZonaFrancaManaus)]
    [InlineData("51", CodigoContribuicaoSocialApurada.CumulativaAliquotaBasica)]
    [InlineData("52", CodigoContribuicaoSocialApurada.CumulativaAliquotasDiferenciadas)]
    [InlineData("53", CodigoContribuicaoSocialApurada.CumulativaAliquotaUnidadeMedida)]
    [InlineData("54", CodigoContribuicaoSocialApurada.CumulativaAliquotaBasicaAtividadeImobiliaria)]
    [InlineData("71", CodigoContribuicaoSocialApurada.ScpIncidenciaNaoCumulativa)]
    [InlineData("72", CodigoContribuicaoSocialApurada.ScpIncidenciaCumulativa)]
    public void CodigoContribuicaoSocialApurada_Roundtrip_MapeiaCodigo(string codigo, CodigoContribuicaoSocialApurada esperado)
    {
        _catalogo.TentarObter("M610".AsSpan(), out var meta);
        var registro = (RegistroM610)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, codigo.AsSpan());

        registro.CodCont.Should().Be(esperado);
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
