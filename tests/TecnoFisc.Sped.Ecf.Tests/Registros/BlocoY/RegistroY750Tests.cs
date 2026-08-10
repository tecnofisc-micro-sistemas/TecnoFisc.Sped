using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote2;

public sealed class RegistroY750Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY750()
    {
        AssertRegistroEcf.CodesAreImplemented("Y750");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroY750(), "Y750", "0:N");
    }

    [Fact]
    public void Parser_PreservaCodigoDescricaoEValorCalculadoDinamicos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y750|000001|DESCRICAO CALCULADA SEM LIMITE FIXO|R$ 1.234,56|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY750>().Which;
        registro.CampoCodigo.Should().Be("000001");
        registro.Descricao.Should().Be("DESCRICAO CALCULADA SEM LIMITE FIXO");
        registro.Valor.Should().Be("R$ 1.234,56");
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
