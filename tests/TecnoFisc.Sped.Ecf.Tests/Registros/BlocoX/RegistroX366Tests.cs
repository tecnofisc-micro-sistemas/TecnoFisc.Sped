using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote2;

public sealed class RegistroX366Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX366(), "X366", "0:N");
    }

    [Fact]
    public void Parser_PreservaLinhaDinamicaRelacionadaAEntidade()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X366|000002|RELACAO DINAMICA|VALOR ALFANUMERICO - 001|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX366>().Which;
        registro.CampoCodigo.Should().Be("000002");
        registro.Descricao.Should().Be("RELACAO DINAMICA");
        registro.Valor.Should().Be("VALOR ALFANUMERICO - 001");
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
