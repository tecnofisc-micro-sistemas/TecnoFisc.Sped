using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Core.Tests.Parser;

public sealed class SnifferSpedTests
{
    [Fact]
    public void MetadadosArquivoSped_ArmazenaValores()
    {
        var metadados = new MetadadosArquivoSped(
            ProjetoSped.EfdContribuicoes,
            6,
            EncodingSped.Latin1,
            "|0000|006|0|||01022025|28022025|EMPRESA|11222333000181|MG|3126901||00|2|",
            "006");

        metadados.Projeto.Should().Be(ProjetoSped.EfdContribuicoes);
        metadados.VersaoLeiaute.Should().Be(6);
        metadados.EncodingDetectado.Should().BeSameAs(EncodingSped.Latin1);
        metadados.CodigoVersaoDeclarado.Should().Be("006");
    }
}
