using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.220 - exercita a forma do <see cref="Registro1200"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 272-273): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1200Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1200).Assembly);

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
    public void Atributo_Declara1200_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1200).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1200");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1200ComSeisCamposNaOrdem()
    {
        _catalogo.TentarObter("1200".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1200");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodAjApur",
            "SldCred",
            "CredApr",
            "CredReceb",
            "CredUtil",
            "SldCredFim",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 6));
        meta.Campos.Should().OnlyContain(c => c.Obrigatorio);
        meta.Campos[0].Tamanho.Should().Be(8);
        meta.Campos.Skip(1).Should().OnlyContain(c => c.Decimais == 2);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1200".AsSpan(), out var meta);
        var registro = (Registro1200)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "SP099999".AsSpan());
        meta.Campos[1].Definidor(registro, "1000,50".AsSpan());
        meta.Campos[2].Definidor(registro, "250,25".AsSpan());
        meta.Campos[3].Definidor(registro, "100,10".AsSpan());
        meta.Campos[4].Definidor(registro, "300,30".AsSpan());
        meta.Campos[5].Definidor(registro, "1050,55".AsSpan());

        registro.CodAjApur.Should().Be("SP099999");
        registro.SldCred.Should().Be(1000.50m);
        registro.CredApr.Should().Be(250.25m);
        registro.CredReceb.Should().Be(100.10m);
        registro.CredUtil.Should().Be(300.30m);
        registro.SldCredFim.Should().Be(1050.55m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1200".AsSpan(), out var meta);
        var registro = (Registro1200)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.CodAjApur.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1200|SP099999|1000,50|250,25|100,10|300,30|1050,55|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CreditosZerados_PreservaTextoCanonico()
    {
        const string sped = "|1200|MG019999|0,00|0,00|0,00|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
