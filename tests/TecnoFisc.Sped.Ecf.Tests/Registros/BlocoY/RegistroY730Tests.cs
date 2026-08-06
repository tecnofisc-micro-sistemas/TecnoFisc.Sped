using System.Reflection;

using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote2;

public sealed class RegistroY730Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY730()
    {
        AssertRegistroEcf.CodesAreImplemented("Y730");
    }

    [Fact]
    public void Registro_ConformeManifestoEDataUsaFormatoExato()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroY730(), "Y730", "0:N");
        var campo = typeof(RegistroY730).GetProperty(nameof(RegistroY730.Data))!
            .GetCustomAttribute<CampoSpedAttribute>()!;
        campo.Formato.Should().Be("ddMMyyyy");
    }

    [Fact]
    public void TipoDestinatario_CoincideComDominioCompletoPfPj()
    {
        typeof(TipoDestinatarioDeducao).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(campo => campo.GetCustomAttribute<SpedValorAttribute>()!.Valor)
            .Should().Equal("PF", "PJ");
    }

    [Fact]
    public void Parser_PreservaCodigosEDocumentoCompostosELeDataValorEObservacaoLonga()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y730|0010|0050|31122025|PJ|00394460000141|-999,99|PROCESSO JUDICIAL E TRIBUNAL SEM LIMITE FIXO|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY730>().Which;
        registro.Deducao.Should().Be("0010");
        registro.Tipo.Should().Be("0050");
        registro.Data.Should().Be(new DateOnly(2025, 12, 31));
        registro.TipoDestinatario.Should().Be(TipoDestinatarioDeducao.PessoaJuridica);
        registro.Destinatario.Should().Be("00394460000141");
        registro.Valor.Should().Be(-999.99m);
        registro.Observacao.Should().Be("PROCESSO JUDICIAL E TRIBUNAL SEM LIMITE FIXO");
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Theory]
    [InlineData("PF", "52998224725")]
    [InlineData("PJ", "00394460000141")]
    public void Parser_PreservaCpfOuCnpjSemNormalizacao(string tipo, string documento)
    {
        var registro = new ParserEcf().ParseLinha(
            $"|Y730|10|50|31122025|{tipo}|{documento}|1,00||").Valor
            .Should().BeOfType<RegistroY730>().Which;

        registro.Destinatario.Should().Be(documento);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_DataDominioEValorInvalidos_RegistramErrosDeFormato()
    {
        var registro = new ParserEcf().ParseLinha(
            "|Y730|10|50|20251231|XX|DOCUMENTO|VALOR||").Valor
            .Should().BeOfType<RegistroY730>().Which;

        registro.ErrosDeFormato.Select(erro => erro.Campo).Should().Contain([
            nameof(RegistroY730.Data), nameof(RegistroY730.TipoDestinatario), nameof(RegistroY730.Valor),
        ]);
    }
}
