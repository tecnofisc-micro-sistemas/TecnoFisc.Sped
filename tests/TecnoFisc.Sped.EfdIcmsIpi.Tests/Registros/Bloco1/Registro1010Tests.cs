using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.216 - exercita a forma do <see cref="Registro1010"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 268-269): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1010Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1010).Assembly);

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
    public void Atributo_Declara1010_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1010).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1010");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1010ComTrezeCamposNaOrdem()
    {
        _catalogo.TentarObter("1010".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1010");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndExp",
            "IndCcrf",
            "IndComb",
            "IndUsina",
            "IndVa",
            "IndEe",
            "IndCart",
            "IndForm",
            "IndAer",
            "IndGiaf1",
            "IndGiaf3",
            "IndGiaf4",
            "IndRestRessarcComplIcms",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]);
        meta.Campos.Should().OnlyContain(c => c.Obrigatorio);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1010".AsSpan(), out var meta);
        var registro = (Registro1010)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "S".AsSpan());
        meta.Campos[1].Definidor(registro, "N".AsSpan());
        meta.Campos[2].Definidor(registro, "S".AsSpan());
        meta.Campos[3].Definidor(registro, "N".AsSpan());
        meta.Campos[4].Definidor(registro, "S".AsSpan());
        meta.Campos[5].Definidor(registro, "N".AsSpan());
        meta.Campos[6].Definidor(registro, "S".AsSpan());
        meta.Campos[7].Definidor(registro, "N".AsSpan());
        meta.Campos[8].Definidor(registro, "S".AsSpan());
        meta.Campos[9].Definidor(registro, "N".AsSpan());
        meta.Campos[10].Definidor(registro, "S".AsSpan());
        meta.Campos[11].Definidor(registro, "N".AsSpan());
        meta.Campos[12].Definidor(registro, "S".AsSpan());

        registro.IndExp.Should().Be(IndicadorSimNao.Sim);
        registro.IndCcrf.Should().Be(IndicadorSimNao.Nao);
        registro.IndComb.Should().Be(IndicadorSimNao.Sim);
        registro.IndUsina.Should().Be(IndicadorSimNao.Nao);
        registro.IndVa.Should().Be(IndicadorSimNao.Sim);
        registro.IndEe.Should().Be(IndicadorSimNao.Nao);
        registro.IndCart.Should().Be(IndicadorSimNao.Sim);
        registro.IndForm.Should().Be(IndicadorSimNao.Nao);
        registro.IndAer.Should().Be(IndicadorSimNao.Sim);
        registro.IndGiaf1.Should().Be(IndicadorSimNao.Nao);
        registro.IndGiaf3.Should().Be(IndicadorSimNao.Sim);
        registro.IndGiaf4.Should().Be(IndicadorSimNao.Nao);
        registro.IndRestRessarcComplIcms.Should().Be(IndicadorSimNao.Sim);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1010".AsSpan(), out var meta);
        var registro = (Registro1010)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.IndExp.Should().Be(default(IndicadorSimNao));
        registro.IndCcrf.Should().Be(default(IndicadorSimNao));
        registro.IndComb.Should().Be(default(IndicadorSimNao));
        registro.IndUsina.Should().Be(default(IndicadorSimNao));
        registro.IndVa.Should().Be(default(IndicadorSimNao));
        registro.IndEe.Should().Be(default(IndicadorSimNao));
        registro.IndCart.Should().Be(default(IndicadorSimNao));
        registro.IndForm.Should().Be(default(IndicadorSimNao));
        registro.IndAer.Should().Be(default(IndicadorSimNao));
        registro.IndGiaf1.Should().Be(default(IndicadorSimNao));
        registro.IndGiaf3.Should().Be(default(IndicadorSimNao));
        registro.IndGiaf4.Should().Be(default(IndicadorSimNao));
        registro.IndRestRessarcComplIcms.Should().Be(default(IndicadorSimNao));
    }

    [Theory]
    [InlineData(IndicadorSimNao.Nao, "N")]
    [InlineData(IndicadorSimNao.Sim, "S")]
    public void Serializar_IndicadorSimNao_RetornaCodigo(IndicadorSimNao indicador, string esperado)
    {
        _catalogo.TentarObter("1010".AsSpan(), out var meta);
        var registro = (Registro1010)meta!.Fabrica();
        registro.IndExp = indicador;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1010|S|N|S|N|S|N|S|N|S|N|S|N|S|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemObrigatoriedades_PreservaTextoCanonico()
    {
        const string sped = "|1010|N|N|N|N|N|N|N|N|N|N|N|N|N|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
