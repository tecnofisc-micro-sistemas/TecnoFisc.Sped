using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote2;

public sealed class RegistroY681Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY681()
    {
        AssertRegistroEcf.CodesAreImplemented("Y681");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroY681(), "Y681", "0:N");
    }

    [Fact]
    public void Parser_PreservaCodigoDescricaoEValorDaTabelaDinamica()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y681|000001|DESCRICAO DINAMICA SEM LIMITE FIXO|R$ -1.234,56|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY681>().Which;
        registro.CampoCodigo.Should().Be("000001");
        registro.Descricao.Should().Be("DESCRICAO DINAMICA SEM LIMITE FIXO");
        registro.Valor.Should().Be("R$ -1.234,56");
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_OptionaisVazios_PreservaNulos()
    {
        var registro = new ParserEcf().ParseLinha("|Y681|000002|||").Valor
            .Should().BeOfType<RegistroY681>().Which;

        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}
