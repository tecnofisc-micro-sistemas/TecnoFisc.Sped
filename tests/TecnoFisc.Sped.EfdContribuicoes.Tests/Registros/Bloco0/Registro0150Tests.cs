using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

public sealed class Registro0150Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0150).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigo0150_Nivel3_Bloco0()
    {
        var atributo = typeof(Registro0150).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0150");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0150ComDozeCamposNaOrdem()
    {
        _catalogo.TentarObter("0150".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0150");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodPart",
            "Nome",
            "CodPais",
            "Cnpj",
            "Cpf",
            "Ie",
            "CodMun",
            "Suframa",
            "End",
            "Num",
            "Compl",
            "Bairro",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0150".AsSpan(), out var meta);
        var registro = (Registro0150)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "FORN001".AsSpan());
        meta.Campos[1].Definidor(registro, "FORNECEDOR LTDA".AsSpan());
        meta.Campos[2].Definidor(registro, "01058".AsSpan());
        meta.Campos[3].Definidor(registro, "11222333000181".AsSpan());
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[5].Definidor(registro, "123456789".AsSpan());
        meta.Campos[6].Definidor(registro, "4205407".AsSpan());
        meta.Campos[7].Definidor(registro, "200400000".AsSpan());
        meta.Campos[8].Definidor(registro, "RUA DAS FLORES".AsSpan());
        meta.Campos[9].Definidor(registro, "100".AsSpan());
        meta.Campos[10].Definidor(registro, "SALA 5".AsSpan());
        meta.Campos[11].Definidor(registro, "CENTRO".AsSpan());

        registro.CodPart.Should().Be("FORN001");
        registro.Nome.Should().Be("FORNECEDOR LTDA");
        registro.CodPais.Should().Be("01058");
        registro.Cnpj.Should().NotBeNull();
        registro.Cnpj!.Value.ToString().Should().Be("11222333000181");
        registro.Cpf.Should().BeNull();
        registro.Ie.Should().Be("123456789");
        registro.CodMun.Should().Be("4205407");
        registro.Suframa.Should().Be("200400000");
        registro.End.Should().Be("RUA DAS FLORES");
        registro.Num.Should().Be("100");
        registro.Compl.Should().Be("SALA 5");
        registro.Bairro.Should().Be("CENTRO");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0150".AsSpan(), out var meta);
        var registro = (Registro0150)meta!.Fabrica();

        // Campos opcionais: CNPJ, CPF, IE, COD_MUN, SUFRAMA, END, NUM, COMPL, BAIRRO
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.Cnpj.Should().BeNull();
        registro.Cpf.Should().BeNull();
        registro.Ie.Should().BeNull();
        registro.CodMun.Should().BeNull();
        registro.Suframa.Should().BeNull();
        registro.End.Should().BeNull();
        registro.Num.Should().BeNull();
        registro.Compl.Should().BeNull();
        registro.Bairro.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|0150|FORN001|FORNECEDOR LTDA|01058|11222333000181||123456789|4205407|200400000|RUA DAS FLORES|100|SALA 5|CENTRO|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ParticipantePessoaFisica_PreservaTextoCanonico()
    {
        // pessoa fisica: sem CNPJ, com CPF; SUFRAMA/END/NUM/COMPL/BAIRRO vazios
        const string sped =
            "|0150|CLI001|JOAO DA SILVA|01058||52998224725||4205407||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ParticipanteExterior_SemCnpjCpfCodMun_PreservaTextoCanonico()
    {
        const string sped =
            "|0150|EXT001|EMPRESA EXTERIOR|00249||||||||||\r\n";

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
