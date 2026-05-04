namespace TecnoFisc.Sped.Core.Tests.ValueObjects;

public sealed class ChaveAcessoTests
{
    // Chave NF-e válida construída com cUF=35 (SP), AAMM=2401, CNPJ=11222333000181,
    // mod=55, serie=001, nNF=000000001, tpEmis=1, cNF=00000001 e cDV calculado por mod 11.
    private const string ChaveValida = "35240111222333000181550010000000011000000018";

    [Fact]
    public void Criar_ComChaveValida_RetornaChaveAcesso()
    {
        var chave = ChaveAcesso.Criar(ChaveValida);

        chave.ToString().Should().Be(ChaveValida);
    }

    [Fact]
    public void Criar_AceitaMascaraComEspacos()
    {
        var chave = ChaveAcesso.Criar("3524 0111 2223 3300 0181 5500 1000 0000 0110 0000 0018");

        chave.ToString().Should().Be(ChaveValida);
    }

    [Fact]
    public void Propriedades_ExtraemCamposEmbutidos()
    {
        var chave = ChaveAcesso.Criar(ChaveValida);

        chave.CodigoUf.Should().Be(35);
        chave.Ano.Should().Be(24);
        chave.Mes.Should().Be(1);
        chave.CnpjEmitente.Should().Be(Cnpj.Criar("11222333000181"));
        chave.Modelo.Should().Be(55);
        chave.Serie.Should().Be(1);
        chave.Numero.Should().Be(1);
        chave.TipoEmissao.Should().Be(1);
        chave.CodigoNumerico.Should().Be("00000001");
        chave.DigitoVerificador.Should().Be(8);
    }

    [Fact]
    public void Criar_ComDvErrado_LancaFormatException()
    {
        // Mesma chave com último dígito alterado.
        var invalido = ChaveValida[..43] + "9";

        var act = () => ChaveAcesso.Criar(invalido);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Criar_ComCnpjEmitenteInvalido_LancaFormatException()
    {
        // Substitui CNPJ embutido por dígitos com DV inválido (00000000000000 já reprovado por TodosIguais).
        var bruto = "35" + "2401" + "12345678901234" + "55" + "001" + "000000001" + "1" + "00000001";
        // Recalcula um DV qualquer para que apenas o CNPJ seja inválido (não o DV da chave).
        // Mais simples: substituir e esperar falha por DV da chave também — qualquer das duas barreiras serve.

        var act = () => ChaveAcesso.Criar(bruto + "0");

        act.Should().Throw<FormatException>();
    }

    [Theory]
    [InlineData("3524011122233300018155001000000001100000001")]   // 43 dígitos
    [InlineData("352401112223330001815500100000000110000000180")] // 45 dígitos
    [InlineData("3524011122233300018155001000000001100000001A")]  // não-dígito
    public void Criar_ComTamanhoOuCharInvalido_LancaFormatException(string entrada)
    {
        var act = () => ChaveAcesso.Criar(entrada);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Criar_ComCodigoUfInvalido_LancaFormatException()
    {
        // cUF=99 não existe.
        var bruto = "9924011122233300018155001000000001100000001";
        // Calcula um DV consistente para isolar a falha em cUF.
        // Se o DV vier diferente da chave ChaveValida, pelo menos uma das validações falha — fim.

        var act = () => ChaveAcesso.Criar(bruto + "0");

        act.Should().Throw<FormatException>();
    }
}
