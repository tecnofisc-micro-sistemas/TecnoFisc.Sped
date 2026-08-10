using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote2;

public sealed class RegistroX365Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX365(), "X365", "0:N");
    }

    [Theory]
    [InlineData("00A9Z9", "ENTIDADE INTERNACIONAL")]
    [InlineData("000001", "")]
    public void Parser_PreservaIdentificadorLexicalENomeOpcional(string identificador, string nome)
    {
        var resultado = new ParserEcf().ParseLinha($"|X365|{identificador}|{nome}|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX365>().Which;
        registro.Identificador.Should().Be(identificador);
        registro.NomeEnt.Should().Be(string.IsNullOrEmpty(nome) ? null : nome);
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
