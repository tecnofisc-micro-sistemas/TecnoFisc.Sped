using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Atributos;

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
            Nome = "CODIGO",
        };

        atributo.Ordem.Should().Be(3);
        atributo.Tamanho.Should().Be(15);
        atributo.Decimais.Should().Be(2);
        atributo.Obrigatorio.Should().BeTrue();
        atributo.Formato.Should().Be("ddMMyyyy");
        atributo.Nome.Should().Be("CODIGO");
    }

    [Fact]
    public void CampoSpedAttribute_QuandoApenasOrdem_DemaisFicamComDefault()
    {
        var atributo = new CampoSpedAttribute { Ordem = 1 };

        atributo.Tamanho.Should().Be(0);
        atributo.Decimais.Should().Be(0);
        atributo.Obrigatorio.Should().BeFalse();
        atributo.Formato.Should().BeNull();
        atributo.Nome.Should().BeNull();
        atributo.DesdeVersao.Should().Be(0);
    }

    [Fact]
    public void CampoSpedAttribute_AceitaDesdeVersao()
    {
        var atributo = new CampoSpedAttribute { Ordem = 27, DesdeVersao = 312 };

        atributo.DesdeVersao.Should().Be(312);
    }

    [Fact]
    public void RegistroSpedAttribute_DefaultIntroduzidoEmZero()
    {
        var atributo = new RegistroSpedAttribute { Codigo = "C100", Nivel = 2, Bloco = "C" };

        atributo.IntroduzidoEm.Should().Be(0);
    }

    [Fact]
    public void RegistroSpedAttribute_AceitaIntroduzidoEm()
    {
        var atributo = new RegistroSpedAttribute
        {
            Codigo = "Z100",
            Nivel = 2,
            Bloco = "Z",
            IntroduzidoEm = 310,
        };

        atributo.IntroduzidoEm.Should().Be(310);
    }

    [Fact]
    public void BlocoSpedAttribute_PreencheIdentificador()
    {
        var atributo = new BlocoSpedAttribute { Identificador = "C" };

        atributo.Identificador.Should().Be("C");
    }
}
