using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco9;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco9;

public sealed class Registro9900Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistro9900()
    {
        AssertRegistroEcf.CodesAreImplemented("9900");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new Registro9900(), "9900", "0:N");
    }

    [Fact]
    public void Parser_LeContagemEIdentificacaoDaTabelaDinamica()
    {
        var resultado = new ParserEcf().ParseLinha("|9900|U100|302|0011|SPEDECF_DINAMICA_U100_A|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<Registro9900>().Which;
        registro.RegBlc.Should().Be("U100");
        registro.QtdRegBlc.Should().Be(302);
        registro.Versao.Should().Be("0011");
        registro.IdTabDin.Should().Be("SPEDECF_DINAMICA_U100_A");
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var registro = new ParserEcf().ParseLinha("|9900|0000|INVALIDA|||").Valor
            .Should().BeOfType<Registro9900>().Which;

        registro.ErrosDeFormato.Should().ContainSingle(erro =>
            erro.Campo == nameof(Registro9900.QtdRegBlc) && erro.ValorBruto == "INVALIDA");
    }
}
