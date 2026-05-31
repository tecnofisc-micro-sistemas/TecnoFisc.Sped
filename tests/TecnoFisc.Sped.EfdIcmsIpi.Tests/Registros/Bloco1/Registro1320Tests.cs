using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.226 - exercita a forma do <see cref="Registro1320"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 277-278): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1320Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1320).Assembly);

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
    public void Atributo_Declara1320_Nivel4_Bloco1()
    {
        var atributo = typeof(Registro1320).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1320");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1320ComDezCamposNaOrdem()
    {
        _catalogo.TentarObter("1320".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1320");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumBico",
            "NrInterv",
            "MotInterv",
            "NomInterv",
            "CnpjInterv",
            "CpfInterv",
            "ValFecha",
            "ValAbert",
            "VolAferi",
            "VolVendas",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 10));
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "NumBico",
            "ValFecha",
            "ValAbert",
            "VolVendas",
        ]);
        meta.Campos[2].Tamanho.Should().Be(50);
        meta.Campos[3].Tamanho.Should().Be(30);
        meta.Campos[4].Tamanho.Should().Be(14);
        meta.Campos[5].Tamanho.Should().Be(11);
        meta.Campos.Where(c => c.Nome is "ValFecha" or "ValAbert" or "VolAferi" or "VolVendas")
            .Should().OnlyContain(c => c.Decimais == 3);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1320".AsSpan(), out var meta);
        var registro = (Registro1320)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12".AsSpan());
        meta.Campos[1].Definidor(registro, "345".AsSpan());
        meta.Campos[2].Definidor(registro, "MANUTENCAO PREVENTIVA".AsSpan());
        meta.Campos[3].Definidor(registro, "TECNICO AUTORIZADO".AsSpan());
        meta.Campos[4].Definidor(registro, "11222333000181".AsSpan());
        meta.Campos[5].Definidor(registro, "52998224725".AsSpan());
        meta.Campos[6].Definidor(registro, "1250,500".AsSpan());
        meta.Campos[7].Definidor(registro, "1000,000".AsSpan());
        meta.Campos[8].Definidor(registro, "5,000".AsSpan());
        meta.Campos[9].Definidor(registro, "245,500".AsSpan());

        registro.NumBico.Should().Be(12);
        registro.NrInterv.Should().Be(345);
        registro.MotInterv.Should().Be("MANUTENCAO PREVENTIVA");
        registro.NomInterv.Should().Be("TECNICO AUTORIZADO");
        registro.CnpjInterv.Should().Be(Cnpj.Create("11222333000181"));
        registro.CpfInterv.Should().Be(Cpf.Create("52998224725"));
        registro.ValFecha.Should().Be(1250.500m);
        registro.ValAbert.Should().Be(1000.000m);
        registro.VolAferi.Should().Be(5.000m);
        registro.VolVendas.Should().Be(245.500m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNuloOuZero()
    {
        _catalogo.TentarObter("1320".AsSpan(), out var meta);
        var registro = (Registro1320)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.NumBico.Should().Be(0);
        registro.NrInterv.Should().BeNull();
        registro.MotInterv.Should().BeNull();
        registro.NomInterv.Should().BeNull();
        registro.CnpjInterv.Should().BeNull();
        registro.CpfInterv.Should().BeNull();
        registro.ValFecha.Should().Be(0m);
        registro.ValAbert.Should().Be(0m);
        registro.VolAferi.Should().BeNull();
        registro.VolVendas.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1320|12|345|MANUTENCAO PREVENTIVA|TECNICO AUTORIZADO|11222333000181|52998224725|1250,500|1000,000|5,000|245,500|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemIntervencao_PreservaTextoCanonico()
    {
        const string sped = "|1320|3||||||875,250|500,000||375,250|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
