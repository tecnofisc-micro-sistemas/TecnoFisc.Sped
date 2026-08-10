using TecnoFisc.Sped.Ecf.Tests.Versionamento;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

/// <summary>
/// As duas origens de <see cref="RegistroNaoReconhecido"/> passam a ser separáveis por um
/// discriminador tipado, sem casar substring na mensagem em português do diagnóstico.
/// </summary>
public sealed class MotivoNaoReconhecimentoTests
{
    [Fact]
    public async Task RegistroForaDeVigencia_TemMotivoPosteriorAVersaoDeclarada()
    {
        // Y730 foi introduzido no leiaute 12; num arquivo de leiaute 9 é sentinela por vigência.
        var registros = await FixtureEcf.ReadAsync(9, "|Y730|1|");

        var sentinela = registros.OfType<RegistroNaoReconhecido>().Single();
        sentinela.Codigo.Should().Be("Y730");
        sentinela.Motivo.Should().Be(MotivoNaoReconhecimento.PosteriorAVersaoDeclarada);
    }

    [Fact]
    public async Task CodigoDesconhecido_TemMotivoCodigoDesconhecido()
    {
        // Leiaute 13 está fora da faixa modelada: código desconhecido degrada para sentinela.
        var registros = await FixtureEcf.ReadAsync(13, "|X999|conteudo novo|");

        var sentinela = registros.OfType<RegistroNaoReconhecido>().Single();
        sentinela.Codigo.Should().Be("X999");
        sentinela.Motivo.Should().Be(MotivoNaoReconhecimento.CodigoDesconhecido);
    }
}
