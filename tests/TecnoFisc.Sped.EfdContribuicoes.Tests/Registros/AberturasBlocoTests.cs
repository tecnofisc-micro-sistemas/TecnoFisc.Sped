using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros;

/// <summary>
/// Sub-stages 4.002, 4.022, 4.030, 4.078, 4.101, 4.127, 4.136, 4.169, 4.177 e 4.200 —
/// registros de abertura de bloco (X001). Todos têm a mesma forma: REG (ordem 1) +
/// IND_MOV (ordem 2, 1 caractere, valores [0,1]). Como são mecanicamente idênticos,
/// os testes são parametrizados por tipo.
/// </summary>
public sealed class AberturasBlocoTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0001).Assembly);

    public static TheoryData<Type, string, string> Aberturas() => new()
    {
        { typeof(Registro0001), "0001", "0" },
        { typeof(RegistroA001), "A001", "A" },
        { typeof(RegistroC001), "C001", "C" },
        { typeof(RegistroD001), "D001", "D" },
        { typeof(RegistroF001), "F001", "F" },
        { typeof(RegistroI001), "I001", "I" },
        { typeof(RegistroM001), "M001", "M" },
        { typeof(RegistroP001), "P001", "P" },
        { typeof(Registro1001), "1001", "1" },
        { typeof(Registro9001), "9001", "9" },
    };

    public static TheoryData<string> Codigos()
    {
        var data = new TheoryData<string>();
        foreach (var codigo in new[] { "0001", "A001", "C001", "D001", "F001", "I001", "M001", "P001", "1001", "9001" })
            data.Add(codigo);
        return data;
    }

    [Theory]
    [MemberData(nameof(Aberturas))]
    public void Atributo_DeclaraCodigoNivelEBlocoEsperados(
        Type tipo, string codigoEsperado, string blocoEsperado)
    {
        var atributo = tipo.GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be(codigoEsperado);
        atributo.Nivel.Should().Be(1);
        atributo.Bloco.Should().Be(blocoEsperado);
    }

    [Theory]
    [MemberData(nameof(Codigos))]
    public void Catalogo_ExpoeApenasIndMovComOrdemDois(string codigo)
    {
        _catalogo.TentarObter(codigo.AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be(codigo);
        meta.Campos.Select(c => c.Nome).Should().Equal(["IndMov"]);
        meta.Campos[0].Ordem.Should().Be(2);
        meta.Campos[0].Tipo.Should().Be<IndicadorMovimentoBloco>();
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[0].Tamanho.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(Codigos))]
    public async Task RoundTrip_ComDados_PreservaTextoCanonico(string codigo)
    {
        string sped = $"|{codigo}|0|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [MemberData(nameof(Codigos))]
    public async Task RoundTrip_SemDados_PreservaTextoCanonico(string codigo)
    {
        string sped = $"|{codigo}|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0001", IndicadorMovimentoBloco.ComDados, "0")]
    [InlineData("A001", IndicadorMovimentoBloco.SemDados, "1")]
    [InlineData("9001", IndicadorMovimentoBloco.SemDados, "1")]
    public void Serializar_IndMov_RetornaCodigoNumerico(
        string codigo, IndicadorMovimentoBloco valor, string esperado)
    {
        _catalogo.TentarObter(codigo.AsSpan(), out var meta);
        var registro = meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ((int)valor).ToString().AsSpan());
        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
