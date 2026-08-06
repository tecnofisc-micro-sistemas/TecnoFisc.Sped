using System.Reflection;

using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote1;

public sealed class RegistroY630Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY630()
    {
        AssertRegistroEcf.CodesAreImplemented("Y630");
    }

    [Theory]
    [InlineData(nameof(RegistroY630.DatAbert), typeof(DateOnly), true)]
    [InlineData(nameof(RegistroY630.DatEncer), typeof(DateOnly?), false)]
    public void DatasDoFundo_UsamDateOnlyEFormatoExato(
        string nomePropriedade,
        Type tipo,
        bool obrigatorio)
    {
        PropertyInfo propriedade = typeof(RegistroY630).GetProperty(nomePropriedade)!;
        CampoSpedAttribute campo = propriedade.GetCustomAttribute<CampoSpedAttribute>()!;

        propriedade.PropertyType.Should().Be(tipo);
        campo.Tamanho.Should().Be(8);
        campo.Formato.Should().Be("ddMMyyyy");
        campo.Obrigatorio.Should().Be(obrigatorio);
    }

    [Fact]
    public void Parser_LeCnpjContagensPatrimonioEDatas()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y630|44444444000191|100|5000000|100000000,25|10012010|31122025|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY630>().Which;
        registro.Cnpj.Should().Be(Cnpj.Create("44444444000191"));
        registro.QteQuot.Should().Be(100);
        registro.QteQuota.Should().Be(5000000);
        registro.PatrFinPer.Should().Be(100000000.25m);
        registro.DatAbert.Should().Be(new DateOnly(2010, 1, 10));
        registro.DatEncer.Should().Be(new DateOnly(2025, 12, 31));
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_CnpjContagensPatrimonioEDatasInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y630|INVALIDO|MUITOS|QUOTAS|PATRIMONIO|20250101|31132025|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY630>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                nameof(RegistroY630.Cnpj),
                nameof(RegistroY630.QteQuot),
                nameof(RegistroY630.QteQuota),
                nameof(RegistroY630.PatrFinPer),
                nameof(RegistroY630.DatAbert),
                nameof(RegistroY630.DatEncer),
            ]);
    }
}
