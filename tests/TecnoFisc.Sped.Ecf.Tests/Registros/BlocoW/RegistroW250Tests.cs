using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoW;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoW;

public sealed class RegistroW250Tests
{
    private const string LinhaCompleta =
        "|W250|NL|ENTIDADE ALFA|TIN-DE-0001|DE|LEI-0001|DE|LEI|OECD303|" +
        "RUA ALFA 100|4930123456789|contato@alfa.test|S|N|N|S|N|N|N|N|N|N|N|N|N|||";

    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasesAcentuados()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroW250(), "W250", "1:N");
    }

    [Fact]
    public void Parser_LeEntidadeTinNiEnderecoAtividadesEOptionais()
    {
        var resultado = new ParserEcf().ParseLinha(LinhaCompleta);

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroW250>().Which;
        registro.JurDiferente.Should().Be("NL");
        registro.Nome.Should().Be("ENTIDADE ALFA");
        registro.Tin.Should().Be("TIN-DE-0001");
        registro.JurisdicaoTin.Should().Be("DE");
        registro.Ni.Should().Be("LEI-0001");
        registro.JurisdicaoNi.Should().Be("DE");
        registro.TipoNi.Should().Be("LEI");
        registro.TipEnd.Should().Be(TipoEnderecoDpp.Oecd303Comercial);
        registro.Endereço.Should().Be("RUA ALFA 100");
        registro.NumTel.Should().Be("4930123456789");
        registro.Email.Should().Be("contato@alfa.test");
        registro.Ativ1.Should().Be(IndicadorSimNao.Sim);
        registro.Ativ4.Should().Be(IndicadorSimNao.Sim);
        registro.Ativ13.Should().Be(IndicadorSimNao.Nao);
        registro.DescOutros.Should().BeNull();
        registro.Observação.Should().BeNull();
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Theory]
    [InlineData("NOTIN")]
    [InlineData("12345678000195")]
    [InlineData("TIN-US-A9")]
    public void Parser_PreservaTinComoStringGenerica(string tin)
    {
        string linha =
            $"|W250||ENTIDADE|{tin}|ZZ||||OECD302|ENDERECO|||" +
            "S|N|N|N|N|N|N|N|N|N|N|N|N|||";

        var resultado = new ParserEcf().ParseLinha(linha);

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroW250>().Which;
        registro.Tin.Should().Be(tin);
        registro.JurisdicaoTin.Should().Be("ZZ");
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Theory]
    [InlineData("OECD301", TipoEnderecoDpp.Oecd301)]
    [InlineData("OECD302", TipoEnderecoDpp.Oecd302Residencial)]
    [InlineData("OECD303", TipoEnderecoDpp.Oecd303Comercial)]
    [InlineData("OECD304", TipoEnderecoDpp.Oecd304)]
    [InlineData("OECD305", TipoEnderecoDpp.Oecd305)]
    public void Parser_LeTiposDeEnderecoRevisados(string valor, TipoEnderecoDpp esperado)
    {
        string linha =
            $"|W250||ENTIDADE|NOTIN|X5||||{valor}|ENDERECO|||" +
            "S|N|N|N|N|N|N|N|N|N|N|N|N|||";

        var resultado = new ParserEcf().ParseLinha(linha);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroW250>()
            .Which.TipEnd.Should().Be(esperado);
    }

    [Theory]
    [InlineData("OECD999", "S", "TIP_END")]
    [InlineData("OECD302", "X", "ATIV_1")]
    public void Parser_DominioInvalido_RegistraErroDeFormato(
        string tipoEndereco,
        string atividade,
        string campo)
    {
        string linha =
            $"|W250||ENTIDADE|NOTIN|X5||||{tipoEndereco}|ENDERECO|||" +
            $"{atividade}|N|N|N|N|N|N|N|N|N|N|N|N|||";

        var resultado = new ParserEcf().ParseLinha(linha);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroW250>()
            .Which.ErrosDeFormato.Should().Contain(erro => erro.Campo == campo);
    }
}
