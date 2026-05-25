using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 10.006 — exercita a forma do <see cref="Registro0150"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 83-85): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class Registro0150Tests
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
    public void Atributo_Declara0150_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0150).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0150");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0150ComDozeCamposNaOrdem()
    {
        _catalogo.TentarObter("0150".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0150");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CodPart", "Nome", "CodPais", "Cnpj", "Cpf", "Nit", "Uf", "Ie", "IeSt", "CodMun", "Im", "Suframa"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0150".AsSpan(), out var meta);
        var registro = (Registro0150)meta!.Fabrica();

        // exemplo adaptado do manual (p. 85); CNPJ e CPF válidos para teste
        meta.Campos[0].Definidor(registro, "03".AsSpan());                  // CodPart
        meta.Campos[1].Definidor(registro, "COLIGADA TESTE S.A.".AsSpan()); // Nome
        meta.Campos[2].Definidor(registro, "01058".AsSpan());               // CodPais
        meta.Campos[3].Definidor(registro, "11111111000191".AsSpan());      // Cnpj
        meta.Campos[4].Definidor(registro, "12345678909".AsSpan());         // Cpf
        meta.Campos[5].Definidor(registro, "12345678901".AsSpan());         // Nit
        meta.Campos[6].Definidor(registro, "35".AsSpan());                  // Uf
        meta.Campos[7].Definidor(registro, "999999".AsSpan());              // Ie
        meta.Campos[8].Definidor(registro, "999998".AsSpan());              // IeSt
        meta.Campos[9].Definidor(registro, "3550508".AsSpan());             // CodMun
        meta.Campos[10].Definidor(registro, "01234567".AsSpan());           // Im
        meta.Campos[11].Definidor(registro, "123456789".AsSpan());          // Suframa

        registro.CodPart.Should().Be("03");
        registro.Nome.Should().Be("COLIGADA TESTE S.A.");
        registro.CodPais.Should().Be("01058");
        registro.Cnpj.Should().NotBeNull();
        registro.Cnpj!.Value.ToString().Should().Be("11111111000191");
        registro.Cpf.Should().NotBeNull();
        registro.Cpf!.Value.ToString().Should().Be("12345678909");
        registro.Nit.Should().Be("12345678901");
        registro.Uf.Should().Be("35");
        registro.Ie.Should().Be("999999");
        registro.IeSt.Should().Be("999998");
        registro.CodMun.Should().Be("3550508");
        registro.Im.Should().Be("01234567");
        registro.Suframa.Should().Be("123456789");
    }

    [Fact]
    public void Definidor_CampoOpcionalVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0150".AsSpan(), out var meta);
        var registro = (Registro0150)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // Cnpj
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // Cpf
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // Nit
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // Ie
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty); // Suframa

        registro.Cnpj.Should().BeNull();
        registro.Cpf.Should().BeNull();
        registro.Nit.Should().BeNull();
        registro.Ie.Should().BeNull();
        registro.Suframa.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CNPJ 11.111.111/0001-91 e CPF 123.456.789-09 são válidos para teste
        const string sped =
            "|0150|03|COLIGADA TESTE S.A.|01058|11111111000191|12345678909|12345678901|35|999999|999998|3550508|01234567|123456789|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SomenteObrigatorios_PreservaTextoCanonico()
    {
        // apenas COD_PART, NOME e COD_PAIS preenchidos; demais campos vazios
        const string sped = "|0150|03|COLIGADA TESTE S.A.|01058||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ParticipanteEstrangeiro_SemUfCodMun_PreservaTextoCanonico()
    {
        // participante estrangeiro: COD_PAIS ≠ Brasil → UF e COD_MUN não preenchidos
        const string sped = "|0150|EXT01|EMPRESA ARGENTINA LTDA|10720||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
