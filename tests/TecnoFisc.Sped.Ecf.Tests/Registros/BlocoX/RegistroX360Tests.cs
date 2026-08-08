using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote2;

public sealed class RegistroX360Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX360(), "X360", "0:N");
    }

    [Fact]
    public void Parser_PreservaCodigoDescricaoEValorDeTabelaDinamicaSemCoercao()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X360|000001|DESCRICAO DINAMICA|R$ -1.234,56 (USD)|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX360>().Which;
        registro.CampoCodigo.Should().Be("000001");
        registro.Descricao.Should().Be("DESCRICAO DINAMICA");
        registro.Valor.Should().Be("R$ -1.234,56 (USD)");
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
