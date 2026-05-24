using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.006 — exercita a forma do <see cref="Registro0100"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 30): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0100Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0100).Assembly);

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
    public void Atributo_Declara0100_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0100).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0100");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0100Com13CamposNaOrdem()
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

        meta.Campos[0].Definidor(registro, "João da Silva".AsSpan());
        meta.Campos[1].Definidor(registro, "52998224725".AsSpan());
        meta.Campos[2].Definidor(registro, "CRC-SP 12345".AsSpan());
        meta.Campos[3].Definidor(registro, "11222333000181".AsSpan());
        meta.Campos[4].Definidor(registro, "01310100".AsSpan());
        meta.Campos[5].Definidor(registro, "Av. Paulista".AsSpan());
        meta.Campos[6].Definidor(registro, "1000".AsSpan());
        meta.Campos[7].Definidor(registro, "Apto 501".AsSpan());
        meta.Campos[8].Definidor(registro, "Bela Vista".AsSpan());
        meta.Campos[9].Definidor(registro, "01130000000".AsSpan());
        meta.Campos[10].Definidor(registro, "01130000001".AsSpan());
        meta.Campos[11].Definidor(registro, "joao@escritorio.com.br".AsSpan());
        meta.Campos[12].Definidor(registro, "3550308".AsSpan());

        registro.Nome.Should().Be("João da Silva");
        registro.Cpf.Should().Be(Cpf.Create("52998224725"));
        registro.Crc.Should().Be("CRC-SP 12345");
        registro.Cnpj.Should().Be(Cnpj.Create("11222333000181"));
        registro.Cep.Should().Be("01310100");
        registro.End.Should().Be("Av. Paulista");
        registro.Num.Should().Be("1000");
        registro.Compl.Should().Be("Apto 501");
        registro.Bairro.Should().Be("Bela Vista");
        registro.Fone.Should().Be("01130000000");
        registro.Fax.Should().Be("01130000001");
        registro.Email.Should().Be("joao@escritorio.com.br");
        registro.CodMun.Should().Be("3550308");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0100".AsSpan(), out var meta);
        var registro = (Registro0100)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // CNPJ (OC)
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // CEP (OC)
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // END (OC)
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // NUM (OC)
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // COMPL (OC)
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty); // BAIRRO (OC)
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty); // FONE (OC)
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty); // FAX (OC)

        registro.Cnpj.Should().BeNull();
        registro.Cep.Should().BeNull();
        registro.End.Should().BeNull();
        registro.Num.Should().BeNull();
        registro.Compl.Should().BeNull();
        registro.Bairro.Should().BeNull();
        registro.Fone.Should().BeNull();
        registro.Fax.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0100|João da Silva|52998224725|CRC-SP 12345|11222333000181|01310100|Av. Paulista|1000|Apto 501|Bela Vista|01130000000|01130000001|joao@escritorio.com.br|3550308|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|0100|Maria Contadora|52998224725|CRC-RJ 99999|||||||||contadora@empresa.com|3304557|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
