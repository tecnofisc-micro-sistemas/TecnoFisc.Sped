using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote3;

public sealed class RegistroX470Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX470(), "X470", "0:N");
    }

    [Fact]
    public void Parser_PreservaCodigoDescricaoEValorDinamico()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X470|000007|CAPACITACAO E INCLUSAO|VALOR: 000123 / TEXTO|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX470>().Which;
        registro.CampoCodigo.Should().Be("000007");
        registro.Descricao.Should().Be("CAPACITACAO E INCLUSAO");
        registro.Valor.Should().Be("VALOR: 000123 / TEXTO");
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
