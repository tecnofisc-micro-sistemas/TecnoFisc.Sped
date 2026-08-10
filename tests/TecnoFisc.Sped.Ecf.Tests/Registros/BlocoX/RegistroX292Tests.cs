using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote1;

public sealed class RegistroX292Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX292(), "X292", "0:N");
    }

    [Theory]
    [InlineData("000014", "Royalties", "100000,00")]
    [InlineData("A00001", null, null)]
    public void Parser_PreservaCodigoDinamicoDescricaoEValorComoTexto(
        string codigo,
        string? descricao,
        string? valor)
    {
        var resultado = new ParserEcf().ParseLinha($"|X292|{codigo}|{descricao}|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX292>().Which;
        registro.CampoCodigo.Should().Be(codigo);
        registro.Descricao.Should().Be(descricao);
        registro.Valor.Should().Be(valor);
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
