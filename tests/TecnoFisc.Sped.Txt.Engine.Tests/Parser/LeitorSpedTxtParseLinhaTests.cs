using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

public sealed class LeitorSpedTxtParseLinhaTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000Sintetico).Assembly);

    private static LeitorSpedTxt Leitor => new(_catalogo);   // opções default: ParseLinha é leniente por natureza

    [Fact]
    public void ParseLinha_LinhaLimpa_RetornaOkSemErros()
    {
        var resultado = Leitor.ParseLinha("|C100|0|123|1500,75|5102|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroC100Sintetico>();
        resultado.Valor.ErrosDeFormato.Should().BeEmpty();
        resultado.Valor.Pai.Should().BeNull();              // sem hierarquia
    }

    [Fact]
    public void ParseLinha_CampoRuim_RetornaSucessoComErroNoRegistro()
    {
        var resultado = Leitor.ParseLinha("|C100|0|abc|1500,75|5102|");

        resultado.Sucesso.Should().BeTrue();                // produziu registro
        resultado.Valor.ErrosDeFormato.Should().HaveCount(1);
        resultado.Valor.ErrosDeFormato[0].Campo.Should().Be("CodPart");
        resultado.Valor.ErrosDeFormato[0].ValorBruto.Should().Be("abc");
    }

    [Fact]
    public void ParseLinha_CodigoDesconhecido_RetornaFalha()
    {
        var resultado = Leitor.ParseLinha("|XXXX|foo|");

        resultado.Falha.Should().BeTrue();
        resultado.Erros.Should().ContainSingle();
        resultado.Erros[0].CodigoRegistro.Should().Be("XXXX");
    }

    [Fact]
    public void ParseLinha_LinhaSemPipes_RetornaFalhaComValorBruto()
    {
        var resultado = Leitor.ParseLinha("C100;0;123");

        resultado.Falha.Should().BeTrue();
        resultado.Erros[0].ValorBruto.Should().Be("C100;0;123");
    }
}
