using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 10.005 — exercita a forma do <see cref="Registro0035"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 82): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class Registro0035Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

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

    [Fact]
    public void Atributo_Declara0035_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0035).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0035");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0035ComDoisCamposNaOrdem()
    {
        _catalogo.TentarObter("0035".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0035");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodScp", "NomeScp"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0035".AsSpan(), out var meta);
        var registro = (Registro0035)meta!.Fabrica();

        // O manual (p. 82) traz "11111111000291" mas a descrição diz "11.111.111/0001-91";
        // "11111111000191" é o CNPJ válido utilizado nos demais exemplos do manual.
        meta.Campos[0].Definidor(registro, "11111111000191".AsSpan());
        meta.Campos[1].Definidor(registro, "SCP TESTE 1".AsSpan());

        registro.CodScp.Should().NotBeNull();
        registro.CodScp!.Value.ToString().Should().Be("11111111000191");
        registro.NomeScp.Should().Be("SCP TESTE 1");
    }

    [Fact]
    public void Definidor_CampoNomeVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0035".AsSpan(), out var meta);
        var registro = (Registro0035)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty); // NomeScp

        registro.NomeScp.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CNPJ válido equivalente ao exemplo do manual (p. 82): 11.111.111/0001-91
        const string sped = "|0035|11111111000191|SCP TESTE 1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemNomeScp_PreservaTextoCanonico()
    {
        const string sped = "|0035|11111111000191||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
