using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.239 - exercita a forma do <see cref="Registro1900"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 289): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1900Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1900).Assembly);

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
    public void Atributo_Declara1900_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1900).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1900");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1900Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("1900".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1900");
        meta.Campos.Select(c => c.Nome).Should().Equal(["IndApurIcms", "DescrComplOutApur"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "IndApurIcms", "DescrComplOutApur"
        ]);
        meta.Campos.Select(c => c.Tamanho).Should().Equal([1, 0]);
        meta.Campos.Select(c => c.Decimais).Should().Equal([0, 0]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1900".AsSpan(), out var meta);
        var registro = (Registro1900)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "3".AsSpan());
        meta.Campos[1].Definidor(registro, "Lei estadual 1234/2020".AsSpan());

        registro.IndApurIcms.Should().Be(IndicadorSubApuracaoIcms.Apuracao1);
        registro.DescrComplOutApur.Should().Be("Lei estadual 1234/2020");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1900".AsSpan(), out var meta);
        var registro = (Registro1900)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.IndApurIcms.Should().BeNull();
        registro.DescrComplOutApur.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1900|3|Lei estadual 1234/2020|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComApuracao6_PreservaTextoCanonico()
    {
        const string sped = "|1900|8|Ressarcimento ST conforme norma UF|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("3", IndicadorSubApuracaoIcms.Apuracao1)]
    [InlineData("4", IndicadorSubApuracaoIcms.Apuracao2)]
    [InlineData("5", IndicadorSubApuracaoIcms.Apuracao3)]
    [InlineData("6", IndicadorSubApuracaoIcms.Apuracao4)]
    [InlineData("7", IndicadorSubApuracaoIcms.Apuracao5)]
    [InlineData("8", IndicadorSubApuracaoIcms.Apuracao6)]
    public void IndApurIcms_Definidor_AtribuiValorCorreto(string valor, IndicadorSubApuracaoIcms? esperado)
    {
        _catalogo.TentarObter("1900".AsSpan(), out var meta);
        var registro = (Registro1900)meta!.Fabrica();
        var campo = meta.Campos.First(c => c.Nome == "IndApurIcms");

        campo.Definidor(registro, valor.AsSpan());

        registro.IndApurIcms.Should().Be(esperado);
    }
}
