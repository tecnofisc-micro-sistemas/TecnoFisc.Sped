using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote3;

public sealed class RegistroX490Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX490(), "X490", "0:N");
    }

    [Fact]
    public void Parser_PreservaCamposDinamicosEOptionaisLossless()
    {
        const string valor = "VALOR TEXTUAL 000123 / -9.876,54";
        var completo = new ParserEcf().ParseLinha($"|X490|000001|POLO INDUSTRIAL|{valor}|");
        var vazio = new ParserEcf().ParseLinha("|X490|000002|||");

        completo.Sucesso.Should().BeTrue();
        var registro = completo.Valor.Should().BeOfType<RegistroX490>().Which;
        registro.CampoCodigo.Should().Be("000001");
        registro.Descricao.Should().Be("POLO INDUSTRIAL");
        registro.Valor.Should().Be(valor);
        registro.ErrosDeFormato.Should().BeEmpty();
        vazio.Valor.Should().BeOfType<RegistroX490>().Which.Descricao.Should().BeNull();
        vazio.Valor.Should().BeOfType<RegistroX490>().Which.Valor.Should().BeNull();
    }
}
