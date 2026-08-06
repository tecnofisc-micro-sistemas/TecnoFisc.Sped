using System.Reflection;

using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote1;

public sealed class RegistroM350Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM350(), "M350", "1:N");
    }

    [Fact]
    public void CampoCodigo_UsaAliasNormativoSemColidirComCodigoDoRegistro()
    {
        PropertyInfo propriedade = typeof(RegistroM350).GetProperty(nameof(RegistroM350.CampoCodigo))!;
        CampoSpedAttribute campo = propriedade.GetCustomAttribute<CampoSpedAttribute>()!;

        campo.Nome.Should().Be("CODIGO");
        new RegistroM350().Codigo.Should().Be("M350");
    }

    [Fact]
    public void Parser_LeLancamentoParteAElacsComValorAssinado()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|M350|0138|OUTRAS EXCLUSOES|E|1|-2000,50|HISTORICO LACS|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM350>().Which;
        registro.CampoCodigo.Should().Be("0138");
        registro.TipoLancamento.Should().Be(TipoLancamentoParteA.Exclusao);
        registro.IndRelacao.Should().Be(IndicadorRelacionamentoParteA.ContaParteB);
        registro.Valor.Should().Be(-2000.50m);
        registro.HistLanLal.Should().Be("HISTORICO LACS");
    }

    [Fact]
    public void Parser_LeRotuloPrevistoPelasRegrasETabelaDinamicaOficiais()
    {
        var resultado = new ParserEcf().ParseLinha("|M350|0001|ROTULO|R||654,32||");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM350>()
            .Which.TipoLancamento.Should().Be(TipoLancamentoParteA.Rotulo);
    }

    [Fact]
    public void Parser_TokenDesconhecido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|M350|0001||X||||");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM350>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain(nameof(RegistroM350.TipoLancamento));
    }
}
