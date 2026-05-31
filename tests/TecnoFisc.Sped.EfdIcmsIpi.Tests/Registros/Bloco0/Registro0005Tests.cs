using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.004 — exercita a forma do <see cref="Registro0005"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 29): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0005Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0005).Assembly);

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
    public void Atributo_Declara0005_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0005).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0005");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0005ComNoveCamposNaOrdem()
    {
        _catalogo.TentarObter("0005".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0005");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "Fantasia",
            "Cep",
            "End",
            "Num",
            "Compl",
            "Bairro",
            "Fone",
            "Fax",
            "Email",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0005".AsSpan(), out var meta);
        var registro = (Registro0005)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "EMPRESA FANTASIA".AsSpan());
        meta.Campos[1].Definidor(registro, "01310100".AsSpan());
        meta.Campos[2].Definidor(registro, "RUA DAS FLORES".AsSpan());
        meta.Campos[3].Definidor(registro, "100".AsSpan());
        meta.Campos[4].Definidor(registro, "SALA 1".AsSpan());
        meta.Campos[5].Definidor(registro, "JARDINS".AsSpan());
        meta.Campos[6].Definidor(registro, "11999990000".AsSpan());
        meta.Campos[7].Definidor(registro, "1199990001".AsSpan());
        meta.Campos[8].Definidor(registro, "empresa@teste.com".AsSpan());

        registro.Fantasia.Should().Be("EMPRESA FANTASIA");
        registro.Cep.Should().Be("01310100");
        registro.End.Should().Be("RUA DAS FLORES");
        registro.Num.Should().Be("100");
        registro.Compl.Should().Be("SALA 1");
        registro.Bairro.Should().Be("JARDINS");
        registro.Fone.Should().Be("11999990000");
        registro.Fax.Should().Be("1199990001");
        registro.Email.Should().Be("empresa@teste.com");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0005".AsSpan(), out var meta);
        var registro = (Registro0005)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // Num
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // Compl
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // Fone
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // Fax
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty); // Email

        registro.Num.Should().BeNull();
        registro.Compl.Should().BeNull();
        registro.Fone.Should().BeNull();
        registro.Fax.Should().BeNull();
        registro.Email.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|0005|EMPRESA FANTASIA|01310100|RUA DAS FLORES|100|SALA 1|JARDINS|11999990000|1199990001|empresa@teste.com|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped =
            "|0005|EMPRESA TESTE|04548001|AV BRIGADEIRO FARIA LIMA|||JARDIM PAULISTANO||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
