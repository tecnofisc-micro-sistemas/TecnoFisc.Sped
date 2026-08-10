using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoQ;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoQ;

public sealed class RegistroQ100Tests
{
    [Fact]
    public void Registro_ConformeManifestoEDeclaraDataNumericaComoDateOnlyDDMMYYYY()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroQ100(), "Q100", "0:N");

        var catalogo = new CatalogoSpedGerado();
        catalogo.TentarObter("Q100", out var metadados).Should().BeTrue();
        metadados!.Campos.Select(campo => campo.Nome)
            .Should().Equal("DATA", "NUM_DOC", "HIST", "VL_ENTRADA", "VL_SAIDA", "SLD_FIN");
        metadados.Campos[0].Tipo.Should().Be<DateOnly>();
        metadados.Campos[0].Formato.Should().Be("ddMMyyyy");
    }

    [Fact]
    public void Parser_LeDataDocumentoHistoricoValoresESaldoComSinal()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Q100|29022024|000001-A|RECEBIMENTO 00042|+1250,75||-0000100,25|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroQ100>().Which;
        registro.Data.Should().Be(new DateOnly(2024, 2, 29));
        registro.NumDoc.Should().Be("000001-A");
        registro.Hist.Should().Be("RECEBIMENTO 00042");
        registro.VlEntrada.Should().Be(1250.75m);
        registro.VlSaida.Should().BeNull();
        registro.SldFin.Should().Be(-100.25m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_PreservaOpcionaisVaziosEValorDeSaida()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Q100|01032025||PAGAMENTO SEM DOCUMENTO||50,25|950,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroQ100>().Which;
        registro.NumDoc.Should().BeNull();
        registro.VlEntrada.Should().BeNull();
        registro.VlSaida.Should().Be(50.25m);
        registro.SldFin.Should().Be(950m);
    }

    [Fact]
    public void Parser_DataEDecimaisInvalidos_RegistramTodosOsErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Q100|29022025|DOC|HISTORICO|1.000,00|--50,25|INVALIDO|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroQ100>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Equal(
                "DATA",
                "VL_ENTRADA",
                "VL_SAIDA",
                "SLD_FIN");
    }

    [Fact]
    public void Parser_NaoExecutaRegrasFiscaisDoLivroCaixa()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Q100|01012025|DOC-LOGICO|SALDO DELIBERADAMENTE INCONSISTENTE|10,00|20,00|999,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroQ100>().Which;
        registro.VlEntrada.Should().Be(10m);
        registro.VlSaida.Should().Be(20m);
        registro.SldFin.Should().Be(999m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
