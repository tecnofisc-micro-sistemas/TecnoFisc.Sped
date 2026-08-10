using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote3;

public sealed class RegistroX420Tests
{
    [Fact]
    public void Registro_ConformeManifestoComSeteMontantesOpcionais()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX420(), "X420", "0:N");
    }

    [Fact]
    public void Parser_LeTipoPaisEscalasSinaisEOptionaisSemPerda()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X420|P|005|1234,56||-2,50|0,01|||7,25|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX420>().Which;
        registro.TipRoy.Should().Be(TipoRoyalty.Pago);
        registro.Pais.Should().Be("005");
        registro.VlExplDirSw.Should().Be(1234.56m);
        registro.VlExplDirAut.Should().BeNull();
        registro.VlExplMarca.Should().Be(-2.50m);
        registro.VlExplPat.Should().Be(0.01m);
        registro.VlExplKnow.Should().BeNull();
        registro.VlExplFranq.Should().BeNull();
        registro.VlExplInt.Should().Be(7.25m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_TipoEMontanteInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X420|X|005|INVALIDO|||||||");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX420>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                "TIP_ROY",
                "VL_EXPL_DIR_SW",
            ]);
    }
}
