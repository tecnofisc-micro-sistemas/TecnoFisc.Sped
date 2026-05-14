using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.225 - exercita a forma do <see cref="Registro1310"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 276-277): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1310Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1310).Assembly);

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
    public void Atributo_Declara1310_Nivel3_Bloco1()
    {
        var atributo = typeof(Registro1310).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1310");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1310ComNoveCamposNaOrdem()
    {
        _catalogo.TentarObter("1310".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1310");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumTanque",
            "EstqAbert",
            "VolEntr",
            "VolDisp",
            "VolSaidas",
            "EstqEscr",
            "ValAjPerda",
            "ValAjGanho",
            "FechFisico",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 9));
        meta.Campos.Should().OnlyContain(c => c.Obrigatorio);
        meta.Campos[0].Tamanho.Should().Be(3);
        meta.Campos.Skip(1).Should().OnlyContain(c => c.Tamanho == 0);
        meta.Campos.Skip(1).Should().OnlyContain(c => c.Decimais == 3);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1310".AsSpan(), out var meta);
        var registro = (Registro1310)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "001".AsSpan());
        meta.Campos[1].Definidor(registro, "700,125".AsSpan());
        meta.Campos[2].Definidor(registro, "150,250".AsSpan());
        meta.Campos[3].Definidor(registro, "850,375".AsSpan());
        meta.Campos[4].Definidor(registro, "400,100".AsSpan());
        meta.Campos[5].Definidor(registro, "450,275".AsSpan());
        meta.Campos[6].Definidor(registro, "0,500".AsSpan());
        meta.Campos[7].Definidor(registro, "0,250".AsSpan());
        meta.Campos[8].Definidor(registro, "450,025".AsSpan());

        registro.NumTanque.Should().Be("001");
        registro.EstqAbert.Should().Be(700.125m);
        registro.VolEntr.Should().Be(150.250m);
        registro.VolDisp.Should().Be(850.375m);
        registro.VolSaidas.Should().Be(400.100m);
        registro.EstqEscr.Should().Be(450.275m);
        registro.ValAjPerda.Should().Be(0.500m);
        registro.ValAjGanho.Should().Be(0.250m);
        registro.FechFisico.Should().Be(450.025m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNuloOuZero()
    {
        _catalogo.TentarObter("1310".AsSpan(), out var meta);
        var registro = (Registro1310)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.NumTanque.Should().BeNull();
        registro.EstqAbert.Should().Be(0m);
        registro.VolEntr.Should().Be(0m);
        registro.VolDisp.Should().Be(0m);
        registro.VolSaidas.Should().Be(0m);
        registro.EstqEscr.Should().Be(0m);
        registro.ValAjPerda.Should().Be(0m);
        registro.ValAjGanho.Should().Be(0m);
        registro.FechFisico.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1310|001|700,125|150,250|850,375|400,100|450,275|0,500|0,250|450,025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_TanqueAgrupadoSemAjustes_PreservaTextoCanonico()
    {
        const string sped = "|1310|T02|1000,000|500,000|1500,000|800,000|700,000|0,000|0,000|700,000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
