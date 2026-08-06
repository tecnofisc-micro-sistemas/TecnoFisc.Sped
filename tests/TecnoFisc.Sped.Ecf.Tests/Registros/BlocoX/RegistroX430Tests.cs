using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote3;

public sealed class RegistroX430Tests
{
    [Fact]
    public void Registro_ConformeManifestoComSeisMontantesOpcionais()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX430(), "X430", "0:N");
    }

    [Fact]
    public void Parser_LePaisMontantesSinaisEOptionaisSemPerda()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X430|005|100,00||-3,25|4,00||5,50|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX430>().Which;
        registro.Pais.Should().Be("005");
        registro.VlServAssist.Should().Be(100m);
        registro.VlServSemAssist.Should().BeNull();
        registro.VlServSemAssistExt.Should().Be(-3.25m);
        registro.VlJuro.Should().Be(4m);
        registro.VlDemaisJuros.Should().BeNull();
        registro.VlDivid.Should().Be(5.50m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_MontanteInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|X430|076|INVALIDO||||||");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX430>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroX430.VlServAssist));
    }
}
