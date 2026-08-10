using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf.Tests.Versionamento;

public sealed class LeiauteForaDaFaixaTests
{
    [Theory]
    [InlineData("0008", true)]
    [InlineData("0012", true)]
    [InlineData("0007", false)]
    [InlineData("0013", false)]
    [InlineData("ABCD", false)]
    public void IsLeiauteConhecido_RefleteAFaixaDoLayoutEcf(string codVer, bool esperado)
        => new Registro0000 { CodVer = codVer }.IsLeiauteConhecido.Should().Be(esperado);

    [Fact]
    public async Task Leitura_DeLeiauteForaDaFaixa_AvisaNoZeroZeroZeroZeroSemAbortar()
    {
        var registros = await FixtureEcf.ReadAsync(13, "|0001|0|");

        var zero = registros.OfType<Registro0000>().Single();
        var erro = zero.ErrosDeFormato.Should().ContainSingle().Which;
        erro.Mensagem.Should().Contain("fora da faixa");
        erro.ValorBruto.Should().Be("0013");
        registros.Should().Contain(registro => registro.Codigo == "0001");
    }

    [Fact]
    public async Task Leitura_DeLeiauteConhecido_NaoAvisa()
    {
        var registros = await FixtureEcf.ReadAsync(12, "|0001|0|");

        registros.OfType<Registro0000>().Single().ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public async Task Leitura_DeLeiaute13ComRegistroNovo_ViraSentinelaEmVezDeAbortar()
    {
        var registros = await FixtureEcf.ReadAsync(13, "|X999|conteudo novo|");

        var sentinela = registros.OfType<RegistroNaoReconhecido>().Should().ContainSingle().Subject;
        sentinela.Codigo.Should().Be("X999");
        sentinela.LinhaCrua.Should().Be("|X999|conteudo novo|");
    }

    [Fact]
    public async Task Leitura_DeLeiauteConhecidoComCodigoDesconhecido_ContinuaAbortando()
    {
        var act = async () => await FixtureEcf.ReadAsync(12, "|X999|conteudo novo|");

        await act.Should().ThrowAsync<ErroLayoutSpedException>();
    }

    /// <summary>
    /// <c>COD_VER</c> ilegível não é leiaute novo, é arquivo inválido: o <c>0000</c> recebe um
    /// diagnóstico dizendo que a vigência não será aplicada — antes esse subconjunto passava em
    /// silêncio e o arquivo era lido como se fosse leiaute 12.
    /// </summary>
    [Theory]
    [InlineData("ABCD")]
    [InlineData("")]
    [InlineData("012")]
    [InlineData("00123")]
    public async Task Leitura_ComCodVerIlegivel_AvisaNoZeroZeroZeroZero(string codVer)
    {
        var registros = await FixtureEcf.ReadAsync(codVer, "|0001|0|");

        var zero = registros.OfType<Registro0000>().Single();
        zero.VersaoLeiaute.Should().Be(0);
        var erro = zero.ErrosDeFormato.Should().ContainSingle().Which;
        erro.Campo.Should().Be("COD_VER");
        erro.Mensagem.Should().Contain("ilegível").And.Contain("vigência");
    }

    /// <summary>
    /// E o diagnóstico não afrouxa nada: sem versão legível, o modo estrito continua valendo e um
    /// código fora do catálogo segue abortando, ao contrário do que acontece com um leiaute fora
    /// da faixa (que tem versão positiva e é lido em modo tolerante).
    /// </summary>
    [Fact]
    public async Task Leitura_ComCodVerIlegivelECodigoDesconhecido_ContinuaAbortando()
    {
        var act = async () => await FixtureEcf.ReadAsync("ABCD", "|X999|conteudo novo|");

        await act.Should().ThrowAsync<ErroLayoutSpedException>();
    }
}
