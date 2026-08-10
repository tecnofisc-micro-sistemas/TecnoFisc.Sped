using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Abstracoes;

public sealed class RegistroSpedTests
{
    [Fact]
    public void ColunasNaoModeladas_EhVaziaPorPadrao()
        => new RegistroDeTeste().ColunasNaoModeladas.Should().BeEmpty();

    private sealed class RegistroDeTeste : RegistroSped
    {
        public override string Codigo => "TST2";
    }
}
