using System.Reflection;

using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote1;

public sealed class RegistroY640Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY640()
    {
        AssertRegistroEcf.CodesAreImplemented("Y640");
    }

    [Fact]
    public void DominioFechadoDeCondicao_CoincideComTabelaCompletaDoManual()
    {
        typeof(CondicaoDeclaranteConsorcio)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(campo => campo.GetCustomAttribute<SpedValorAttribute>()!.Valor)
            .Should().Equal("1", "2");
    }

    [Fact]
    public void Parser_LeConsorcioCondicaoCnpjsEReceitas()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y640|44444444000191|1|500000,00|22222222000191|-400000,25|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY640>().Which;
        registro.Cnpj.Should().Be(Cnpj.Create("44444444000191"));
        registro.CondDecl.Should().Be(CondicaoDeclaranteConsorcio.Lider);
        registro.VlCons.Should().Be(500000m);
        registro.CnpjLid.Should().Be(Cnpj.Create("22222222000191"));
        registro.VlDecl.Should().Be(-400000.25m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_ReceitaOpcionalVazia_PreservaNuloSemAplicarCalculo()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y640|44444444000191|2||22222222000191|0,00|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY640>().Which.VlCons.Should().BeNull();
    }

    [Fact]
    public void Parser_CnpjsCondicaoEReceitasInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y640|CONSORCIO|9|TOTAL|LIDER|DECLARANTE|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY640>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                nameof(RegistroY640.Cnpj),
                nameof(RegistroY640.CondDecl),
                nameof(RegistroY640.VlCons),
                nameof(RegistroY640.CnpjLid),
                nameof(RegistroY640.VlDecl),
            ]);
    }
}
