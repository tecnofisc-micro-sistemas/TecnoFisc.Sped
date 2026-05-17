using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.242 - exercita a forma do <see cref="Registro1921"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 293): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1921Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1921).Assembly);

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
    public void Atributo_Declara1921_Nivel5_Bloco1()
    {
        var atributo = typeof(Registro1921).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1921");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1921Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("1921".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1921");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodAjApur", "DescrComplAj", "VlAjApur"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
        meta.Campos.Select(c => c.Tamanho).Should().Equal([8, 0, 0]);
        meta.Campos.Select(c => c.Decimais).Should().Equal([0, 0, 2]);
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal(["CodAjApur", "VlAjApur"]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1921".AsSpan(), out var meta);
        var registro = (Registro1921)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "SP020100".AsSpan());
        meta.Campos[1].Definidor(registro, "Outros creditos".AsSpan());
        meta.Campos[2].Definidor(registro, "1250,75".AsSpan());

        registro.CodAjApur.Should().Be("SP020100");
        registro.DescrComplAj.Should().Be("Outros creditos");
        registro.VlAjApur.Should().Be(1250.75m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1921".AsSpan(), out var meta);
        var registro = (Registro1921)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.CodAjApur.Should().BeNull();
        registro.DescrComplAj.Should().BeNull();
        registro.VlAjApur.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|1921|SP020100|Outros creditos|1250,75|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemDescricaoComplementar_PreservaTextoCanonico()
    {
        const string sped =
            "|1921|SP050100||500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
