using System.Reflection;

using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote1;

public sealed class RegistroM300Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM300(), "M300", "1:N");
    }

    [Fact]
    public void CampoCodigo_UsaAliasNormativoSemColidirComCodigoDoRegistro()
    {
        PropertyInfo propriedade = typeof(RegistroM300).GetProperty(nameof(RegistroM300.CampoCodigo))!;
        CampoSpedAttribute campo = propriedade.GetCustomAttribute<CampoSpedAttribute>()!;

        campo.Nome.Should().Be("CODIGO");
        new RegistroM300().Codigo.Should().Be("M300");
    }

    [Fact]
    public void DominioDeLeitura_CoincideComRegrasETabelaDinamicaOficiais()
    {
        typeof(TipoLancamentoParteA)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(campo => campo.GetCustomAttribute<SpedValorAttribute>()!.Valor)
            .Should().Equal("A", "E", "P", "R", "L");

        ((int)TipoLancamentoParteA.Lucro).Should().Be(3,
            "o novo token não deve alterar o valor CLR público preexistente");
        ((int)TipoLancamentoParteA.Rotulo).Should().Be(4);
    }

    [Fact]
    public void Parser_LeCodigoDinamicoDominiosValorComSinalEOpcionais()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|M300|0138|OUTRAS EXCLUSOES|E|3|-1000,25|HISTORICO LALUR|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM300>().Which;
        registro.CampoCodigo.Should().Be("0138");
        registro.TipoLancamento.Should().Be(TipoLancamentoParteA.Exclusao);
        registro.IndRelacao.Should().Be(IndicadorRelacionamentoParteA.ContaParteBContaContabil);
        registro.Valor.Should().Be(-1000.25m);
        registro.HistLanLal.Should().Be("HISTORICO LALUR");
    }

    [Fact]
    public void Parser_CamposOpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|M300|0001||||||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM300>().Which;
        registro.Descricao.Should().BeNull();
        registro.TipoLancamento.Should().BeNull();
        registro.IndRelacao.Should().BeNull();
        registro.Valor.Should().BeNull();
        registro.HistLanLal.Should().BeNull();
    }

    [Fact]
    public void Parser_LeRotuloPrevistoPelasRegrasETabelaDinamicaOficiais()
    {
        var resultado = new ParserEcf().ParseLinha("|M300|0001|ROTULO|R||654,32||");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM300>()
            .Which.TipoLancamento.Should().Be(TipoLancamentoParteA.Rotulo);
    }

    [Fact]
    public void Parser_DominiosEValorInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|M300|0001||X|9|INVALIDO||");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM300>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                "TIPO_LANCAMENTO",
                "IND_RELACAO",
                "VALOR",
            ]);
    }
}
