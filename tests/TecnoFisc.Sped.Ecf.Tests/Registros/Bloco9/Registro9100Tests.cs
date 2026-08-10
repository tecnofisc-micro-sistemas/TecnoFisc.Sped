using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco9;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco9;

public sealed class Registro9100Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistro9100()
    {
        AssertRegistroEcf.CodesAreImplemented("9100");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new Registro9100(), "9100", "0:N");
    }

    [Fact]
    public void Parser_LeAvisoSemPerderCodigosZerosOuEscala()
    {
        const string linha =
            "|9100|00123|AVISO SINTETICO|Y600|CPF_CNPJ|-100,25|99,50|001|0001|CC01|R1|000001|A01|11111111000191|0123456|PB01|IRPJ|";

        var resultado = new ParserEcf().ParseLinha(linha);

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<Registro9100>().Which;
        registro.NomRegra.Should().Be("00123");
        registro.MsgRegra.Should().Be("AVISO SINTETICO");
        registro.Registro.Should().Be("Y600");
        registro.Campo.Should().Be("CPF_CNPJ");
        registro.Conteudo.Should().Be(-100.25m);
        registro.ValorEsperado.Should().Be(99.50m);
        registro.PerApur.Should().Be("001");
        registro.CodCta.Should().Be("0001");
        registro.CodCcus.Should().Be("CC01");
        registro.CodCtaRef.Should().Be("R1");
        registro.CampoCodigo.Should().Be("000001");
        registro.NumOrdem.Should().Be("A01");
        registro.CnpjEstab.ToString().Should().Be("11111111000191");
        registro.Cnae.Should().Be("0123456");
        registro.CodCtaB.Should().Be("PB01");
        registro.CodTributo.Should().Be("IRPJ");
    }

    [Fact]
    public void Parser_ConteudoNumericoInvalido_RegistraErroDeFormato()
    {
        var registro = new ParserEcf().ParseLinha(
            "|9100|00123||Y600||INVALIDO||||||||||||").Valor
            .Should().BeOfType<Registro9100>().Which;

        registro.ErrosDeFormato.Should().ContainSingle(erro =>
            erro.Campo == "CONTEÚDO" && erro.ValorBruto == "INVALIDO");
    }
}
