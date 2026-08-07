using System.Reflection;

using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote1;

public sealed class RegistroY620Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY620()
    {
        AssertRegistroEcf.CodesAreImplemented("Y620");
    }

    [Fact]
    public void DominioFechadoDeRelacionamento_CoincideComTabelaCompletaDoManual()
    {
        typeof(TipoRelacionamentoParticipacao)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(campo => campo.GetCustomAttribute<SpedValorAttribute>()!.Valor)
            .Should().Equal("1", "2", "3", "4", "5");
    }

    [Theory]
    [InlineData(nameof(RegistroY620.DtEvento))]
    [InlineData(nameof(RegistroY620.DataAquis))]
    public void DatasDaParticipacao_UsamDateOnlyEFormatoExato(string nomePropriedade)
    {
        PropertyInfo propriedade = typeof(RegistroY620).GetProperty(nomePropriedade)!;
        CampoSpedAttribute campo = propriedade.GetCustomAttribute<CampoSpedAttribute>()!;

        propriedade.PropertyType.Should().Be<DateOnly>();
        campo.Tamanho.Should().Be(8);
        campo.Formato.Should().Be("ddMMyyyy");
        campo.Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Parser_LeRelacionamentoDocumentoValoresAssinadosDatasEIdentificadoresLongos()
    {
        const string numeroCartorio = "REGISTRO-CARTORIO-SEM-LIMITE-FIXO-000000000123";
        const string nomeCartorio = "CARTORIO E ENDERECO COMPLETO SEM LIMITE FIXO";
        const string numeroRfb = "PROCESSO-ELETRONICO-RFB-SEM-LIMITE-FIXO-000987";
        var resultado = new ParserEcf().ParseLinha(
            $"|Y620|01012024|1|105|44444444000191|EMPRESA COLIGADA|1000000,00|-25,50|25,1250|30,0000|-100000,00|31102013|S|{numeroCartorio}|{nomeCartorio}|S|{numeroRfb}|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY620>().Which;
        registro.DtEvento.Should().Be(new DateOnly(2024, 1, 1));
        registro.IndRelac.Should().Be(TipoRelacionamentoParticipacao.Controle);
        registro.Pais.Should().Be("105");
        registro.Cnpj.Should().Be(Cnpj.Create("44444444000191"));
        registro.ValorReais.Should().Be(1000000m);
        registro.ValorEstr.Should().Be(-25.50m);
        registro.PercCapTot.Should().Be(25.1250m);
        registro.PercCapVot.Should().Be(30m);
        registro.ResEqPat.Should().Be(-100000m);
        registro.DataAquis.Should().Be(new DateOnly(2013, 10, 31));
        registro.IndProcCart.Should().Be(IndicadorSimNao.Sim);
        registro.NumProcCart.Should().Be(numeroCartorio);
        registro.NomeCart.Should().Be(nomeCartorio);
        registro.IndProcRfb.Should().Be(IndicadorSimNao.Sim);
        registro.NumProcRfb.Should().Be(numeroRfb);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_DocumentoEIdentificadoresOpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y620|01012024|5|249||EMPRESA EXTERIOR|0,00|0,00|0,0000|0,0000||01012020|N|||N||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY620>().Which;
        registro.Cnpj.Should().BeNull();
        registro.ResEqPat.Should().BeNull();
        registro.NumProcCart.Should().BeNull();
        registro.NomeCart.Should().BeNull();
        registro.NumProcRfb.Should().BeNull();
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_DatasDominioCnpjIndicadoresEValoresInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y620|20250101|9|105|INVALIDO|EMPRESA|REAIS|ESTR|TOTAL|VOT|RESULTADO|31132025|X|||?|PROC|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY620>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                "DT_EVENTO",
                "IND_RELAC",
                "CNPJ",
                "VALOR_REAIS",
                "VALOR_ESTR",
                "PERC_CAP_TOT",
                "PERC_CAP_VOT",
                "RES_EQ_PAT",
                "DATA_AQUIS",
                "IND_PROC_CART",
                "IND_PROC_RFB",
            ]);
    }
}
