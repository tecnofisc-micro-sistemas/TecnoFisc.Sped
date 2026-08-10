namespace TecnoFisc.Sped.Ecf.Tests.Versionamento;

public sealed class LayoutEcfTests
{
    [Fact]
    public void LayoutEcf_DeclaraV008AteV012ComValoresNumericos()
    {
        Enum.GetValues<LayoutEcf>()
            .Select(layout => (Nome: layout.ToString(), Valor: (int)layout))
            .Should().Equal([
                ("V008", 8),
                ("V009", 9),
                ("V010", 10),
                ("V011", 11),
                ("V012", 12),
            ]);
    }
}
