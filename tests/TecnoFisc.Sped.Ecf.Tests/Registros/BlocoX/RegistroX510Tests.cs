using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote4;

public sealed class RegistroX510Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX510(), "X510", "0:N");
    }

    [Fact]
    public void Parser_PreservaCamposDinamicosEOptionaisLossless()
    {
        const string valor = "VALOR TEXTUAL 000987 / -6.543,21";
        var completo = new ParserEcf().ParseLinha(
            $"|X510|ALC-CODIGO-SEM-LIMITE-0002|AREA DE LIVRE COMERCIO|{valor}|");
        var vazio = new ParserEcf().ParseLinha("|X510|ALC-03|||");

        completo.Sucesso.Should().BeTrue();
        var registro = completo.Valor.Should().BeOfType<RegistroX510>().Which;
        registro.CampoCodigo.Should().Be("ALC-CODIGO-SEM-LIMITE-0002");
        registro.Descricao.Should().Be("AREA DE LIVRE COMERCIO");
        registro.Valor.Should().Be(valor);
        registro.ErrosDeFormato.Should().BeEmpty();
        vazio.Valor.Should().BeOfType<RegistroX510>().Which.Descricao.Should().BeNull();
        vazio.Valor.Should().BeOfType<RegistroX510>().Which.Valor.Should().BeNull();
    }
}
