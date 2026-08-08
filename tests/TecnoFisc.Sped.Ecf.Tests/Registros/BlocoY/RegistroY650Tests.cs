using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote1;

public sealed class RegistroY650Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY650()
    {
        AssertRegistroEcf.CodesAreImplemented("Y650");
    }

    [Fact]
    public void Parser_LeParticipanteEReceitaOpcional()
    {
        var completo = new ParserEcf().ParseLinha("|Y650|11111111000191|-100000,25|");
        var vazio = new ParserEcf().ParseLinha("|Y650|11111111000191||");

        completo.Sucesso.Should().BeTrue();
        var registro = completo.Valor.Should().BeOfType<RegistroY650>().Which;
        registro.Cnpj.Should().Be(Cnpj.Create("11111111000191"));
        registro.VlPart.Should().Be(-100000.25m);
        registro.ErrosDeFormato.Should().BeEmpty();
        vazio.Valor.Should().BeOfType<RegistroY650>().Which.VlPart.Should().BeNull();
    }

    [Fact]
    public void Parser_CnpjEReceitaInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|Y650|PARTICIPANTE|RECEITA|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY650>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain(["CNPJ", "VL_PART"]);
    }
}
