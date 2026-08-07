using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote1;

public sealed class RegistroY590Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY590()
    {
        AssertRegistroEcf.CodesAreImplemented("Y590");
    }

    [Fact]
    public void Parser_PreservaTipoDinamicoPaisDiscriminacaoLongaEValores()
    {
        const string tipoAtivo = "00000000000331";
        const string discriminacao = "ATIVO ADQUIRIDO NO EXTERIOR COM IDENTIFICADOR E DESCRICAO SEM LIMITE FIXO";
        var resultado = new ParserEcf().ParseLinha(
            $"|Y590|{tipoAtivo}|001|{discriminacao}|-10,25|300000,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY590>().Which;
        registro.TipAtivo.Should().Be(tipoAtivo);
        registro.Pais.Should().Be("001");
        registro.Discriminacao.Should().Be(discriminacao);
        registro.VlAnt.Should().Be(-10.25m);
        registro.VlAtual.Should().Be(300000m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_ValoresInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|Y590|0331|249|ATIVO|ANTERIOR|ATUAL|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY590>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain(["VL_ANT", "VL_ATUAL"]);
    }
}
