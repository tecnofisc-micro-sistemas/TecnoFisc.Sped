using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros;

/// <summary>
/// Sub-stages 4.021, 4.029, 4.077, 4.100, 4.126, 4.135, 4.168, 4.176, 4.199, 4.202 e
/// 4.203 — registros de encerramento de bloco (X990) e do arquivo (9999). Todos têm a
/// mesma forma (REG + QTD_LIN_X inteiro). Hierarquicamente os X990 são nível 1, e o
/// 9999 é nível 0 (raiz, junto com 0000).
/// </summary>
public sealed class EncerramentosBlocoTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0990).Assembly);

    public static TheoryData<Type, string, string, int> Atributos() => new()
    {
        { typeof(Registro0990), "0990", "0", 1 },
        { typeof(RegistroA990), "A990", "A", 1 },
        { typeof(RegistroC990), "C990", "C", 1 },
        { typeof(RegistroD990), "D990", "D", 1 },
        { typeof(RegistroF990), "F990", "F", 1 },
        { typeof(RegistroI990), "I990", "I", 1 },
        { typeof(RegistroM990), "M990", "M", 1 },
        { typeof(RegistroP990), "P990", "P", 1 },
        { typeof(Registro1990), "1990", "1", 1 },
        { typeof(Registro9990), "9990", "9", 1 },
        { typeof(Registro9999), "9999", "9", 0 },
    };

    public static TheoryData<string, string> CodigoEPropriedade() => new()
    {
        { "0990", "QtdLin0" },
        { "A990", "QtdLinA" },
        { "C990", "QtdLinC" },
        { "D990", "QtdLinD" },
        { "F990", "QtdLinF" },
        { "I990", "QtdLinI" },
        { "M990", "QtdLinM" },
        { "P990", "QtdLinP" },
        { "1990", "QtdLin1" },
        { "9990", "QtdLin9" },
        { "9999", "QtdLin"  },
    };

    public static TheoryData<string> Codigos()
    {
        var data = new TheoryData<string>();
        foreach (var codigo in new[] { "0990", "A990", "C990", "D990", "F990", "I990", "M990", "P990", "1990", "9990", "9999" })
            data.Add(codigo);
        return data;
    }

    [Theory]
    [MemberData(nameof(Atributos))]
    public void Atributo_DeclaraCodigoNivelEBlocoEsperados(
        Type tipo, string codigoEsperado, string blocoEsperado, int nivelEsperado)
    {
        var atributo = tipo.GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be(codigoEsperado);
        atributo.Nivel.Should().Be(nivelEsperado);
        atributo.Bloco.Should().Be(blocoEsperado);
    }

    [Theory]
    [MemberData(nameof(CodigoEPropriedade))]
    public void Catalogo_ExpoeApenasQtdLinComOrdemDois(string codigo, string nomePropriedade)
    {
        _catalogo.TentarObter(codigo.AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be(codigo);
        meta.Campos.Select(c => c.Nome).Should().Equal([nomePropriedade]);
        meta.Campos[0].Ordem.Should().Be(2);
        meta.Campos[0].Tipo.Should().Be<int>();
        meta.Campos[0].Obrigatorio.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(Codigos))]
    public async Task RoundTrip_QtdLinQualquer_PreservaTextoCanonico(string codigo)
    {
        string sped = $"|{codigo}|42|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [MemberData(nameof(Codigos))]
    public async Task RoundTrip_QtdLinZero_PreservaTextoCanonico(string codigo)
    {
        string sped = $"|{codigo}|0|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.WriteAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
