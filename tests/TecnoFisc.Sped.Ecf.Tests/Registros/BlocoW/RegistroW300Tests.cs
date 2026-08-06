using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoW;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoW;

public sealed class RegistroW300Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveMarcadorFinalObrigatorio()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroW300(), "W300", "0:N");
    }

    [Fact]
    public void Manifesto_PreservaSimMinusculoNormativoEmFimObservacao()
    {
        var campo = ManifestoEcf.Carregar().Obter("W300").Fields
            .Single(item => item.Name == "FIM_OBSERVACAO");

        campo.Required.Should().Be("sim");
        campo.ValidValues.Should().Be("[W300FIM]");
    }

    [Fact]
    public void Parser_LeObservacaoAssociadaAJurisdicaoECampos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|W300|DE|S|N|N|S|N|N|N|N|N|N|CRITERIO ALTERADO|W300FIM|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroW300>().Which;
        registro.Jurisdicao.Should().Be("DE");
        registro.IndRecNaoRel.Should().Be(IndicadorSimNao.Sim);
        registro.IndRecRel.Should().Be(IndicadorSimNao.Nao);
        registro.IndRecTotal.Should().Be(IndicadorSimNao.Nao);
        registro.IndLucPrejAntesIr.Should().Be(IndicadorSimNao.Sim);
        registro.IndIrPago.Should().Be(IndicadorSimNao.Nao);
        registro.IndIrDevido.Should().Be(IndicadorSimNao.Nao);
        registro.IndCapSoc.Should().Be(IndicadorSimNao.Nao);
        registro.IndLucAcum.Should().Be(IndicadorSimNao.Nao);
        registro.IndAtivTang.Should().Be(IndicadorSimNao.Nao);
        registro.IndNumEmp.Should().Be(IndicadorSimNao.Nao);
        registro.Observação.Should().Be("CRITERIO ALTERADO");
        registro.FimObservacao.Should().Be("W300FIM");
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_AssociacoesOpcionaisVazias_PermanecemNulas()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|W300||||||||||||OBSERVACAO GLOBAL|W300FIM|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroW300>().Which;
        registro.Jurisdicao.Should().BeNull();
        registro.IndRecNaoRel.Should().BeNull();
        registro.IndNumEmp.Should().BeNull();
        registro.Observação.Should().Be("OBSERVACAO GLOBAL");
        registro.FimObservacao.Should().Be("W300FIM");
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_IndicadorInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|W300|DE|X|N|N|N|N|N|N|N|N|N|OBSERVACAO|W300FIM|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroW300>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroW300.IndRecNaoRel) && erro.ValorBruto == "X");
    }
}
