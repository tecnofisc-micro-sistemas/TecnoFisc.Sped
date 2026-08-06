using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote2;

public sealed class RegistroX400Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX400(), "X400", "0:N");
    }

    [Theory]
    [InlineData("23", "TRANSACOES COM ORGAOS DA ADMINISTRACAO PUBLICA", "")]
    [InlineData("000024", "VENDAS DE BENS TANGIVEIS", "100000,00")]
    public void Parser_PreservaCodigoDescricaoEValorDinamicos(string codigo, string descricao, string valor)
    {
        var resultado = new ParserEcf().ParseLinha($"|X400|{codigo}|{descricao}|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX400>().Which;
        registro.CampoCodigo.Should().Be(codigo);
        registro.Descricao.Should().Be(descricao);
        registro.Valor.Should().Be(string.IsNullOrEmpty(valor) ? null : valor);
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
