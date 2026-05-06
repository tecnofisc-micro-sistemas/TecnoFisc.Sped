using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 4.004 — exercita a forma do <see cref="Registro0100"/> contra o Guia Prático
/// v1.35 (p. 70-71): metadados de catálogo, mapeamento de campos, serialização e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0100Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0100).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigo0100_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0100).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0100");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0100ComTrezeCamposNaOrdem()
    {
        _catalogo.TentarObter("0100".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0100");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "Nome",
            "Cpf",
            "Crc",
            "Cnpj",
            "Cep",
            "End",
            "Num",
            "Compl",
            "Bairro",
            "Fone",
            "Fax",
            "Email",
            "CodMun",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0100".AsSpan(), out var meta);
        var registro = (Registro0100)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "JOAO DA SILVA CONTADOR".AsSpan());
        meta.Campos[1].Definidor(registro, "52998224725".AsSpan());
        meta.Campos[2].Definidor(registro, "SC1234567890123".AsSpan());
        meta.Campos[3].Definidor(registro, "11222333000181".AsSpan());
        meta.Campos[4].Definidor(registro, "88010000".AsSpan());
        meta.Campos[5].Definidor(registro, "RUA DAS FLORES".AsSpan());
        meta.Campos[6].Definidor(registro, "123".AsSpan());
        meta.Campos[7].Definidor(registro, "SALA 201".AsSpan());
        meta.Campos[8].Definidor(registro, "CENTRO".AsSpan());
        meta.Campos[9].Definidor(registro, "04833330000".AsSpan());
        meta.Campos[10].Definidor(registro, "04833330001".AsSpan());
        meta.Campos[11].Definidor(registro, "joao@contabilidade.com".AsSpan());
        meta.Campos[12].Definidor(registro, "4205407".AsSpan());

        registro.Nome.Should().Be("JOAO DA SILVA CONTADOR");
        registro.Cpf.ToString().Should().Be("52998224725");
        registro.Crc.Should().Be("SC1234567890123");
        registro.Cnpj.Should().NotBeNull();
        registro.Cnpj!.Value.ToString().Should().Be("11222333000181");
        registro.Cep.Should().Be("88010000");
        registro.End.Should().Be("RUA DAS FLORES");
        registro.Num.Should().Be("123");
        registro.Compl.Should().Be("SALA 201");
        registro.Bairro.Should().Be("CENTRO");
        registro.Fone.Should().Be("04833330000");
        registro.Fax.Should().Be("04833330001");
        registro.Email.Should().Be("joao@contabilidade.com");
        registro.CodMun.Should().Be("4205407");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("0100".AsSpan(), out var meta);
        var registro = (Registro0100)meta!.Fabrica();

        // Campos opcionais (5-14): CNPJ, CEP, END, NUM, COMPL, BAIRRO, FONE, FAX, EMAIL, COD_MUN
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.Cnpj.Should().BeNull();
        registro.Cep.Should().BeNull();
        registro.End.Should().BeNull();
        registro.Num.Should().BeNull();
        registro.Compl.Should().BeNull();
        registro.Bairro.Should().BeNull();
        registro.Fone.Should().BeNull();
        registro.Fax.Should().BeNull();
        registro.Email.Should().BeNull();
        registro.CodMun.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|0100|JOAO DA SILVA CONTADOR|52998224725|SC1234567890123|11222333000181|88010000|RUA DAS FLORES|123|SALA 201|CENTRO|04833330000|04833330001|joao@contabilidade.com|4205407|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped =
            "|0100|MARIA JOSE SOUZA|52998224725|RJ0000000000000|||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AninhadoSob0001_RespeitaHierarquia()
    {
        const string sped =
            "|0000|006|0|||01012025|31012025|EMPRESA CONTABILIDADE SA|11222333000181|SP|3550308||00|2|\r\n" +
            "|0001|0|\r\n" +
            "|0100|JOAO DA SILVA CONTADOR|52998224725|SC1234567890123|||||||||||\r\n";

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
