using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Versionamento;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

/// <summary>
/// Prova o contrato "nada se perde em silêncio": toda coluna presente na linha que o modelo
/// tipado não representa chega ao consumidor em bruto, com o motivo.
/// </summary>
public sealed class ColunasNaoModeladasTests
{
    [Fact]
    public async Task X450DeLeiaute10_PreservaAsColunasDeDetalheEmBruto()
    {
        var registros = await FixtureEcf.ReadAsync(
            10, "|X450|249|11222333000181|BENEFICIARIO|1234,56|01|N|");

        var x450 = registros.OfType<RegistroX450>().Single();
        x450.Pais.Should().Be("249");
        x450.ColunasNaoModeladas.Select(coluna => (coluna.Posicao, coluna.Valor)).Should().Equal(
            (3, "11222333000181"), (4, "BENEFICIARIO"), (5, "1234,56"), (6, "01"), (7, "N"));
        x450.ColunasNaoModeladas.Should().OnlyContain(
            coluna => coluna.Motivo == MotivoColunaNaoModelada.AlemDoModelo);
    }

    [Fact]
    public async Task RegistroRemovidoNoLeiaute11_PreservaTodasAsColunas()
    {
        var registros = await FixtureEcf.ReadAsync(10, "|X300|000001|EXPORTACAO|1234,56|");

        var x300 = registros.Single(registro => registro.Codigo == "X300");
        x300.ColunasNaoModeladas.Select(coluna => coluna.Valor)
            .Should().Equal("000001", "EXPORTACAO", "1234,56");
        x300.ColunasNaoModeladas.Should().OnlyContain(
            coluna => coluna.Motivo == MotivoColunaNaoModelada.AlemDoModelo);
    }

    [Fact]
    public async Task CampoPosteriorAoLeiauteDeclarado_ViraColunaNaoModelada()
    {
        // 0020 tem 31 colunas de dado (Ordem 2..32). As duas últimas — POSSUI_CEBRAS (Ordem 31,
        // DesdeVersao 10) e CEBAS (Ordem 32, DesdeVersao 12) — não vigoram no leiaute 9.
        var valores = new List<string> { "1", "1" };
        valores.AddRange(Enumerable.Repeat("N", 27));
        valores.Add("S");
        valores.Add("CEBAS-TESTE");
        string linha = "|0020|" + string.Join('|', valores) + "|";

        var registros = await FixtureEcf.ReadAsync(9, linha);

        var registro0020 = registros.Single(registro => registro.Codigo == "0020");
        registro0020.ColunasNaoModeladas.Should().BeEquivalentTo([
            new ColunaNaoModelada(31, "S", MotivoColunaNaoModelada.PosteriorAVersaoDeclarada),
            new ColunaNaoModelada(32, "CEBAS-TESTE", MotivoColunaNaoModelada.PosteriorAVersaoDeclarada),
        ]);
    }

    [Fact]
    public async Task LinhaSemExcedente_NaoRegistraNada()
    {
        var registros = await FixtureEcf.ReadAsync(12, "|0001|0|");

        registros.Should().OnlyContain(registro => registro.ColunasNaoModeladas.Count == 0);
    }
}
