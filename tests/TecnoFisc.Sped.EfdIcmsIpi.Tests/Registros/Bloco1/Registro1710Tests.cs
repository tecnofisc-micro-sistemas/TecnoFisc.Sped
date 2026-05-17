using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.237 - exercita a forma do <see cref="Registro1710"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (pp. 287-288): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1710Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1710).Assembly);

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
    public void Atributo_Declara1710_Nivel3_Bloco1()
    {
        var atributo = typeof(Registro1710).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1710");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1710Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("1710".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1710");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumDocIni", "NumDocFin"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "NumDocIni", "NumDocFin"
        ]);
        meta.Campos.Select(c => c.Tamanho).Should().Equal([12, 12]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1710".AsSpan(), out var meta);
        var registro = (Registro1710)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "45".AsSpan());
        meta.Campos[1].Definidor(registro, "52".AsSpan());

        registro.NumDocIni.Should().Be(45);
        registro.NumDocFin.Should().Be(52);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1710".AsSpan(), out var meta);
        var registro = (Registro1710)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.NumDocIni.Should().Be(0);
        registro.NumDocFin.Should().Be(0);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1710|45|52|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CancelamentoNaoContinuo_PreservaTextoCanonico()
    {
        const string sped = "|1710|45|45|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
