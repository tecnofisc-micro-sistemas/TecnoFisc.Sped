using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Core.Tests.Atributos;

public sealed class AtributosTests
{
    [Fact]
    public void RegistroSpedAttribute_PreencheCodigoNivelBloco()
    {
        var atributo = new RegistroSpedAttribute { Codigo = "C100", Nivel = 2, Bloco = "C" };

        atributo.Codigo.Should().Be("C100");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void CampoSpedAttribute_AceitaPropriedadesOpcionais()
    {
        var atributo = new CampoSpedAttribute
        {
            Ordem = 3,
            Tamanho = 15,
            Decimais = 2,
            Obrigatorio = true,
            Formato = "ddMMyyyy",
        };

        atributo.Ordem.Should().Be(3);
        atributo.Tamanho.Should().Be(15);
        atributo.Decimais.Should().Be(2);
        atributo.Obrigatorio.Should().BeTrue();
        atributo.Formato.Should().Be("ddMMyyyy");
    }

    [Fact]
    public void CampoSpedAttribute_QuandoApenasOrdem_DemaisFicamComDefault()
    {
        var atributo = new CampoSpedAttribute { Ordem = 1 };

        atributo.Tamanho.Should().Be(0);
        atributo.Decimais.Should().Be(0);
        atributo.Obrigatorio.Should().BeFalse();
        atributo.Formato.Should().BeNull();
    }

    [Fact]
    public void BlocoSpedAttribute_PreencheIdentificador()
    {
        var atributo = new BlocoSpedAttribute { Identificador = "C" };

        atributo.Identificador.Should().Be("C");
    }
}
