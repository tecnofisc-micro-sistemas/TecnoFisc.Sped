using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf.Tests;

public sealed class ArquivoEcfTests
{
    [Fact]
    public void EnumerarBlocos_RetornaDezesseteBlocosNaOrdemCanonica()
    {
        var arquivo = new ArquivoEcf();

        arquivo.EnumerarBlocos().Select(bloco => bloco.Identificador)
            .Should().Equal([
                "0", "C", "E", "J", "K", "L", "M", "N", "P",
                "Q", "T", "U", "V", "W", "X", "Y", "9",
            ]);
    }

    [Fact]
    public void Adicionar_CodigoDeBlocoDesconhecido_LancaInvalidOperationException()
    {
        var arquivo = new ArquivoEcf();
        var registro = new RegistroSintetico();

        var act = () => arquivo.Adicionar(registro);

        act.Should().Throw<InvalidOperationException>();
        arquivo.EnumerarRegistros().Should().BeEmpty();
        arquivo.EnumerarBlocos().SelectMany(bloco => bloco.EnumerarRegistros()).Should().BeEmpty();
    }

    private sealed class RegistroSintetico : RegistroSped
    {
        public override string Codigo => "Z001";
    }
}
