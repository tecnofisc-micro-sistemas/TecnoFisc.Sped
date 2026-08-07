using System.Reflection;

using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote2;

public sealed class RegistroY672Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY672()
    {
        AssertRegistroEcf.CodesAreImplemented("Y672");
    }

    [Fact]
    public void Registro_ConformeManifestoCorrigidoComExatos18Campos()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroY672(), "Y672", "0:1");
        var metadados = new CatalogoSpedGerado().EnumerarRegistros()
            .Single(registro => registro.Codigo == "Y672");

        metadados.Campos.Should().HaveCount(17);
        metadados.Campos[^1].Ordem.Should().Be(18);
        metadados.Campos[^1].Tipo.Should().Be<MetodoAvaliacaoEstoque?>();
    }

    [Fact]
    public void MetodoEstoque_ReutilizaDominioSemanticamenteIdenticoDaTabelaCompleta()
    {
        (MetodoAvaliacaoEstoque Valor, string Token, string Significado)[] dominio = [
            (MetodoAvaliacaoEstoque.CustoMedioPonderado, "1", "custo médio ponderado"),
            (MetodoAvaliacaoEstoque.Peps, "2", "PEPS"),
            (MetodoAvaliacaoEstoque.Arbitramento, "3", "arbitramento"),
            (MetodoAvaliacaoEstoque.CustoEspecifico, "4", "custo específico"),
            (MetodoAvaliacaoEstoque.ValorRealizavelLiquido, "5", "valor realizável líquido"),
            (MetodoAvaliacaoEstoque.InventarioPeriodico, "6", "inventário periódico"),
            (MetodoAvaliacaoEstoque.Outros, "7", "outros"),
            (MetodoAvaliacaoEstoque.NaoHa, "8", "não há"),
        ];

        dominio.Select(item => item.Token).Should().Equal("1", "2", "3", "4", "5", "6", "7", "8");
        dominio.Select(item => item.Significado).Should().Equal(
            "custo médio ponderado", "PEPS", "arbitramento", "custo específico",
            "valor realizável líquido", "inventário periódico", "outros", "não há");
        dominio.Select(item => item.Valor.GetType().GetField(item.Valor.ToString())!
                .GetCustomAttribute<SpedValorAttribute>()!.Valor)
            .Should().Equal(dominio.Select(item => item.Token));
    }

    [Fact]
    public void Parser_LeTodosOsValoresEExercitaCampo18()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y672|10000,00|20000,00|1000,00|2000,00|5000,00|6000,00|1000,00|2000,00|1000,00|2000,00|1000,00|2000,00|1000,00|2000,00|10000,00|100000,00|2|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY672>().Which;
        registro.VlCapitalAnt.Should().Be(10000m);
        registro.VlReceitas.Should().Be(10000m);
        registro.TotAtivo.Should().Be(100000m);
        registro.IndAvalEstoq.Should().Be(MetodoAvaliacaoEstoque.Peps);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_TodosOsCamposOpcionaisVazios_PreservaNulos()
    {
        string linha = $"|Y672|{string.Join("|", Enumerable.Repeat(string.Empty, 17))}|";

        var registro = new ParserEcf().ParseLinha(linha).Valor
            .Should().BeOfType<RegistroY672>().Which;

        typeof(RegistroY672).GetProperties()
            .Where(propriedade => propriedade.GetCustomAttribute<CampoSpedAttribute>() is not null)
            .Select(propriedade => propriedade.GetValue(registro))
            .Should().AllSatisfy(valor => valor.Should().BeNull());
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_ValoresEMetodoInvalidos_RegistramErrosDeFormato()
    {
        string linha = $"|Y672|{string.Join("|", Enumerable.Repeat("INVALIDO", 16))}|9|";

        var registro = new ParserEcf().ParseLinha(linha).Valor
            .Should().BeOfType<RegistroY672>().Which;

        registro.ErrosDeFormato.Should().HaveCount(17);
        registro.ErrosDeFormato[^1].Campo.Should().Be("IND_AVAL_ESTOQ");
    }

    [Fact]
    public async Task ReadAsync_ExemploLegadoSobrelongo_RejeitaSemCoagirCamposExtras()
    {
        const string linhaLegada =
            "|Y672|10000,00|20000,00|1000,00|2000,00|5000,00|6000,00|1000,00|2000,00|" +
            "1000,00|2000,00|1000,00|2000,00|1000,00|2000,00|10000,00|100000,00|" +
            "10000,00|10,00|2|2|\r\n";
        var parser = new ParserEcf();

        var diagnostico = parser.ParseLinha(linhaLegada.TrimEnd('\r', '\n')).Valor
            .Should().BeOfType<RegistroY672>().Which;
        diagnostico.VlCapitalAnt.Should().Be(10000m);
        diagnostico.TotAtivo.Should().Be(100000m);
        diagnostico.IndAvalEstoq.Should().BeNull();
        diagnostico.ErrosDeFormato.Should().ContainSingle(erro =>
            erro.Campo == "IND_AVAL_ESTOQ" && erro.ValorBruto == "10000,00");

        var camposMaterializados = typeof(RegistroY672).GetProperties()
            .Where(propriedade => propriedade.GetCustomAttribute<CampoSpedAttribute>() is not null)
            .ToArray();
        camposMaterializados.Should().HaveCount(17);

        await using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(linhaLegada), writable: false);
        var act = async () => await parser.ReadAsync(entrada, TestContext.Current.CancellationToken);

        var assercao = await act.Should().ThrowAsync<ErroFormatoSpedException>();
        assercao.Which.Erro.CodigoRegistro.Should().Be("Y672");
        assercao.Which.Erro.Campo.Should().Be("IND_AVAL_ESTOQ");
        assercao.Which.Erro.ValorBruto.Should().Be("10000,00");
    }
}
