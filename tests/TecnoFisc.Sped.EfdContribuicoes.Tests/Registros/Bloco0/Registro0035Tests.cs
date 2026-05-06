using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 4.003 — exercita a forma do <see cref="Registro0035"/> contra o Guia Prático
/// v1.35 (p. 69-70): metadados de catálogo, mapeamento de campos, serialização e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0035Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0035).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigo0035_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0035).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0035");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0035ComTresCamposNaOrdem()
    {
        _catalogo.TentarObter("0035".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0035");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodScp", "DescScp", "InfComp"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
        meta.Campos[0].Tamanho.Should().Be(14);
        meta.Campos[1].Tamanho.Should().Be(0);
        meta.Campos[2].Tamanho.Should().Be(0);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0035".AsSpan(), out var meta);
        var registro = (Registro0035)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12345678901234".AsSpan());
        meta.Campos[1].Definidor(registro, "SCP IMOBILIARIA EXEMPLO".AsSpan());
        meta.Campos[2].Definidor(registro, "EMPREENDIMENTO XYZ".AsSpan());

        registro.CodScp.Should().Be("12345678901234");
        registro.DescScp.Should().Be("SCP IMOBILIARIA EXEMPLO");
        registro.InfComp.Should().Be("EMPREENDIMENTO XYZ");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0035".AsSpan(), out var meta);
        var registro = (Registro0035)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodScp.Should().BeNull();
        registro.DescScp.Should().BeNull();
        registro.InfComp.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|0035|12345678901234|SCP IMOBILIARIA EXEMPLO|EMPREENDIMENTO XYZ|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemInfComp_PreservaTextoCanonico()
    {
        const string sped =
            "|0035|99999999999999|SCP COMERCIAL ABC||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_PreservaCaracteresLatin1()
    {
        const string sped =
            "|0035|12345678901234|SOCIEDADE DE PARTICIPAÇÃO ESPECÍFICA|OBSERVAÇÃO COMPLEMENTAR|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AninhadoSob0001_RespeitaHierarquia()
    {
        // Cenário do Guia Prático p. 69-70: Registro 0000 com IND_NAT_PJ = 03 obriga
        // a presença de um ou mais 0035 sob o 0001, listando cada SCP em que a PJ
        // participa como sócia ostensiva.
        const string sped =
            "|0000|006|0|||01012025|31012025|EMPRESA SOCIA OSTENSIVA|11222333000181|SP|3550308||03|2|\r\n" +
            "|0001|0|\r\n" +
            "|0035|12345678901234|SCP XXX SOCIA OSTENSIVA|EMPREENDIMENTO 1|\r\n" +
            "|0035|22345678901234|SCP ABC SOCIA OSTENSIVA|EMPREENDIMENTO 2|\r\n" +
            "|0035|32345678901234|SCP WEG SOCIA OSTENSIVA|EMPREENDIMENTO 3|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
