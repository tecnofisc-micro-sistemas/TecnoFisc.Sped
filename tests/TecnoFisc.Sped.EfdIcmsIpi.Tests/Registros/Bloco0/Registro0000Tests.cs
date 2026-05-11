using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.001 — exercita a forma do <see cref="Registro0000"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 26): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0000Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

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
    public void Atributo_Declara0000_Nivel0_Bloco0()
    {
        var atributo = typeof(Registro0000).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0000");
        atributo.Nivel.Should().Be(0);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0000ComQuatorzeCamposNaOrdem()
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0000");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodVer",
            "CodFin",
            "DtIni",
            "DtFin",
            "Nome",
            "Cnpj",
            "Cpf",
            "Uf",
            "Ie",
            "CodMun",
            "Im",
            "Suframa",
            "IndPerfil",
            "IndAtiv",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "306".AsSpan());
        meta.Campos[1].Definidor(registro, "0".AsSpan());
        meta.Campos[2].Definidor(registro, "01012020".AsSpan());
        meta.Campos[3].Definidor(registro, "31122020".AsSpan());
        meta.Campos[4].Definidor(registro, "EMPRESA TESTE SA".AsSpan());
        meta.Campos[5].Definidor(registro, "11222333000181".AsSpan());
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[7].Definidor(registro, "SP".AsSpan());
        meta.Campos[8].Definidor(registro, "123456789".AsSpan());
        meta.Campos[9].Definidor(registro, "3550308".AsSpan());
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[12].Definidor(registro, "A".AsSpan());
        meta.Campos[13].Definidor(registro, "0".AsSpan());

        registro.CodVer.Should().Be("306");
        registro.CodFin.Should().Be(IndicadorFinalidadeArquivo.Original);
        registro.DtIni.Should().Be(new DateOnly(2020, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2020, 12, 31));
        registro.Nome.Should().Be("EMPRESA TESTE SA");
        registro.Cnpj.Should().Be(Cnpj.Criar("11222333000181"));
        registro.Cpf.Should().BeNull();
        registro.Uf.Should().Be("SP");
        registro.Ie.ToString().Should().Be("123456789");
        registro.CodMun.Should().Be("3550308");
        registro.Im.Should().BeNull();
        registro.Suframa.Should().BeNull();
        registro.IndPerfil.Should().Be("A");
        registro.IndAtiv.Should().Be(IndicadorAtividadeIpi.Industrial);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // Cnpj
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);  // Cpf
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty); // Im
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty); // Suframa

        registro.Cnpj.Should().BeNull();
        registro.Cpf.Should().BeNull();
        registro.Im.Should().BeNull();
        registro.Suframa.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorFinalidadeArquivo.Original, "0")]
    [InlineData(IndicadorFinalidadeArquivo.Substituto, "1")]
    public void Serializar_CodFin_RetornaCodigo(IndicadorFinalidadeArquivo finalidade, string esperado)
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000)meta!.Fabrica();
        registro.CodFin = finalidade;

        meta.Campos[1].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorAtividadeIpi.Industrial, "0")]
    [InlineData(IndicadorAtividadeIpi.Outros, "1")]
    public void Serializar_IndAtiv_RetornaCodigo(IndicadorAtividadeIpi atividade, string esperado)
    {
        _catalogo.TentarObter("0000".AsSpan(), out var meta);
        var registro = (Registro0000)meta!.Fabrica();
        registro.IndAtiv = atividade;

        meta.Campos[13].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|0000|306|0|01012020|31122020|EMPRESA TESTE SA|11222333000181||SP|123456789|3550308|||A|0|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCpfECnpjVazio_PreservaTextoCanonico()
    {
        const string sped =
            "|0000|306|0|01012020|31122020|JOAO DA SILVA||12345678909|SP|ISENTO|3550308|||B|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
