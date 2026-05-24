using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.118 — exercita a forma do <see cref="RegistroD120"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 169): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD120Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD120).Assembly);

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
    public void Atributo_DeclaraD120_Nivel4_BlocoD()
    {
        var atributo = typeof(RegistroD120).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D120");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD120Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("D120".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D120");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMunOrig", "CodMunDest", "VeicId", "UfId",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 4));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D120".AsSpan(), out var meta);
        var registro = (RegistroD120)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "3550308".AsSpan()); // CodMunOrig
        meta.Campos[1].Definidor(registro, "3304557".AsSpan()); // CodMunDest
        meta.Campos[2].Definidor(registro, "ABC1234".AsSpan()); // VeicId
        meta.Campos[3].Definidor(registro, "SP".AsSpan());      // UfId

        registro.CodMunOrig.Should().Be(3550308);
        registro.CodMunDest.Should().Be(3304557);
        registro.VeicId.Should().Be("ABC1234");
        registro.UfId.Should().Be("SP");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("D120".AsSpan(), out var meta);
        var registro = (RegistroD120)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty); // CodMunOrig opcional
        meta.Campos[1].Definidor(registro, Span<char>.Empty); // CodMunDest opcional
        meta.Campos[2].Definidor(registro, Span<char>.Empty); // VeicId opcional
        meta.Campos[3].Definidor(registro, Span<char>.Empty); // UfId opcional

        registro.CodMunOrig.Should().BeNull();
        registro.CodMunDest.Should().BeNull();
        registro.VeicId.Should().BeNull();
        registro.UfId.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Complemento de NF de serviço de transporte (cód. 07): São Paulo → Rio de Janeiro, placa SP.
        const string sped = "|D120|3550308|3304557|ABC1234|SP|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemVeiculoUf_PreservaTextoCanonico()
    {
        // Complemento sem identificação de veículo (VEIC_ID e UF_ID omitidos).
        const string sped = "|D120|3550308|3304557|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_MunicipioExterior_PreservaTextoCanonico()
    {
        // Código 9999999 para município no Exterior (conforme orientação do guia).
        const string sped = "|D120|9999999|9999999|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
