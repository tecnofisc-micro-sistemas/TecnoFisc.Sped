using TecnoFisc.Sped.Core.Erros;

namespace TecnoFisc.Sped.Core.Tests.Erros;

public sealed class ResultadoParseTests
{
    [Fact]
    public void Ok_RetornaSucessoComValor()
    {
        var resultado = ResultadoParse<int>.Ok(42);

        resultado.Sucesso.Should().BeTrue();
        resultado.Falha.Should().BeFalse();
        resultado.Valor.Should().Be(42);
        resultado.Erros.Should().BeEmpty();
    }

    [Fact]
    public void Falhar_ComUmErro_RetornaFalhaComErroAcumulado()
    {
        var erro = new ErroFormato(10, "C100", "VlDoc", "valor inválido");

        var resultado = ResultadoParse<string>.Falhar(erro);

        resultado.Sucesso.Should().BeFalse();
        resultado.Falha.Should().BeTrue();
        resultado.Erros.Should().ContainSingle().Which.Should().Be(erro);
    }

    [Fact]
    public void Valor_QuandoFalha_LancaInvalidOperationException()
    {
        var resultado = ResultadoParse<int>.Falhar(new ErroFormato(1, null, null, "x"));

        var act = () => resultado.Valor;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TentarObter_SeguePadraoTryGet()
    {
        var sucesso = ResultadoParse<int>.Ok(7);
        var falha = ResultadoParse<int>.Falhar(new ErroFormato(2, null, null, "x"));

        sucesso.TentarObter(out var valor1).Should().BeTrue();
        valor1.Should().Be(7);

        falha.TentarObter(out var valor2).Should().BeFalse();
        valor2.Should().Be(0);
    }

    [Fact]
    public void Falhar_ComListaNula_LancaArgumentNullException()
    {
        var act = () => ResultadoParse<int>.Falhar((IReadOnlyList<ErroFormato>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ErroFormato_ToString_FormataConformeContexto()
    {
        new ErroFormato(5, null, null, "msg").ToString().Should().Be("Linha 5: msg");
        new ErroFormato(5, "C100", null, "msg").ToString().Should().Be("Linha 5 (C100): msg");
        new ErroFormato(5, "C100", "VlDoc", "msg").ToString().Should().Be("Linha 5 (C100.VlDoc): msg");
    }

    [Fact]
    public void ErroLayout_ToString_FormataConformeContexto()
    {
        new ErroLayout(7, null, "msg").ToString().Should().Be("Linha 7: msg");
        new ErroLayout(7, "X999", "msg").ToString().Should().Be("Linha 7 (X999): msg");
    }
}
