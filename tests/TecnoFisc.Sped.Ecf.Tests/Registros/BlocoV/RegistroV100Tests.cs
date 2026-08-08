using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoV;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoV;

public sealed class RegistroV100Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroV100(), "V100", "1:N");
    }

    [Theory]
    [InlineData("61", "AQUISICAO DE BENS", "10000,00")]
    [InlineData("62", "PAGAMENTO", "-25,50")]
    [InlineData("99", "PERCENTUAL", "12,3400%")]
    public void Parser_PreservaCodigoDescricaoEValorTextual(
        string codigo,
        string descricao,
        string valor)
    {
        var resultado = new ParserEcf().ParseLinha(
            $"|V100|{codigo}|{descricao}|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroV100>().Which;
        registro.CampoCodigo.Should().Be(codigo);
        registro.Descricao.Should().Be(descricao);
        registro.Valor.Should().Be(valor);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_CamposOpcionaisVazios_PermanecemNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|V100|01|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroV100>().Which;
        registro.CampoCodigo.Should().Be("01");
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}
