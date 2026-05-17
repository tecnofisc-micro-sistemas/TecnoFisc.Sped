using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.007 — exercita a forma do <see cref="Registro0150"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 30-31): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0150Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0150).Assembly);

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
    public void Catalogo_ExpoeRegistro0150Com12CamposNaOrdem()
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

        meta.Campos[0].Definidor(registro, "PART001".AsSpan());
        meta.Campos[1].Definidor(registro, "Empresa Teste Ltda".AsSpan());
        meta.Campos[2].Definidor(registro, "01058".AsSpan());
        meta.Campos[3].Definidor(registro, "11222333000181".AsSpan());
        meta.Campos[4].Definidor(registro, "52998224725".AsSpan());
        meta.Campos[5].Definidor(registro, "111222333".AsSpan());
        meta.Campos[6].Definidor(registro, "3550308".AsSpan());
        meta.Campos[7].Definidor(registro, "200012345".AsSpan());
        meta.Campos[8].Definidor(registro, "Rua das Flores".AsSpan());
        meta.Campos[9].Definidor(registro, "100".AsSpan());
        meta.Campos[10].Definidor(registro, "Sala 1".AsSpan());
        meta.Campos[11].Definidor(registro, "Centro".AsSpan());

        registro.CodPart.Should().Be("PART001");
        registro.Nome.Should().Be("Empresa Teste Ltda");
        registro.CodPais.Should().Be("01058");
        registro.Cnpj.Should().Be(Cnpj.Criar("11222333000181"));
        registro.Cpf.Should().Be(Cpf.Criar("52998224725"));
        registro.Ie.Should().Be(InscricaoEstadual.Criar("111222333"));
        registro.CodMun.Should().Be("3550308");
        registro.Suframa.Should().Be("200012345");
        registro.End.Should().Be("Rua das Flores");
        registro.Num.Should().Be("100");
        registro.Compl.Should().Be("Sala 1");
        registro.Bairro.Should().Be("Centro");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0150".AsSpan(), out var meta);
        var registro = (Registro0150)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty); // COD_PART (opcional)
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // CNPJ (OC)
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // CPF (OC)
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // IE (OC)
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // COD_MUN (OC)
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // SUFRAMA (OC)
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty); // NUM (OC)
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty); // COMPL (OC)
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty); // BAIRRO (OC)

        registro.CodPart.Should().BeNull();
        registro.Cnpj.Should().BeNull();
        registro.Cpf.Should().BeNull();
        registro.Ie.Should().BeNull();
        registro.CodMun.Should().BeNull();
        registro.Suframa.Should().BeNull();
        registro.Num.Should().BeNull();
        registro.Compl.Should().BeNull();
        registro.Bairro.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0150|PART001|Empresa Teste Ltda|01058|11222333000181||111222333|3550308|200012345|Rua das Flores|100|Sala 1|Centro|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ParticipanteExterior_PreservaTextoCanonico()
    {
        const string sped = "|0150|EXTPART|Empresa Internacional Inc|00249||||9999999||New York, USA||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
