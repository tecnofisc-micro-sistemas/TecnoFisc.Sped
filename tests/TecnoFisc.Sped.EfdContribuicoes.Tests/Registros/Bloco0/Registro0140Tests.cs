using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

public sealed class Registro0140Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0140).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigo0140_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0140).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0140");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0140ComOitoCamposNaOrdem()
    {
        _catalogo.TentarObter("0140".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0140");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodEst",
            "Nome",
            "Cnpj",
            "Uf",
            "Ie",
            "CodMun",
            "Im",
            "Suframa",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0140".AsSpan(), out var meta);
        var registro = (Registro0140)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "EST001".AsSpan());
        meta.Campos[1].Definidor(registro, "EMPRESA TESTE SA".AsSpan());
        meta.Campos[2].Definidor(registro, "11222333000181".AsSpan());
        meta.Campos[3].Definidor(registro, "SC".AsSpan());
        meta.Campos[4].Definidor(registro, "12345678901234".AsSpan());
        meta.Campos[5].Definidor(registro, "4205407".AsSpan());
        meta.Campos[6].Definidor(registro, "12345678".AsSpan());
        meta.Campos[7].Definidor(registro, "010203040".AsSpan());

        registro.CodEst.Should().Be("EST001");
        registro.Nome.Should().Be("EMPRESA TESTE SA");
        registro.Cnpj.ToString().Should().Be("11222333000181");
        registro.Uf.Should().Be("SC");
        registro.Ie.Should().Be("12345678901234");
        registro.CodMun.Should().Be("4205407");
        registro.Im.Should().Be("12345678");
        registro.Suframa.Should().Be("010203040");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("0140".AsSpan(), out var meta);
        var registro = (Registro0140)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty); // CodEst
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // Ie
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // Im
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // Suframa

        registro.CodEst.Should().BeNull();
        registro.Ie.Should().BeNull();
        registro.Im.Should().BeNull();
        registro.Suframa.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|0140|EST001|EMPRESA TESTE SA|11222333000181|SC|12345678901234|4205407|12345678|010203040|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped =
            "|0140||EMPRESA TESTE SA|11222333000181|SC||4205407|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AninhadoSob0001_RespeitaHierarquia()
    {
        const string sped =
            "|0000|006|0|||01012025|31012025|EMPRESA CONTRIB SA|11222333000181|SP|3550308||00|2|\r\n" +
            "|0001|0|\r\n" +
            "|0140||EMPRESA CONTRIB SA|11222333000181|SP||3550308|||\r\n";

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
