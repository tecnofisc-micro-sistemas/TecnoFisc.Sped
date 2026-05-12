using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoB;

/// <summary>
/// Sub-stage 8.034 — exercita a forma do <see cref="RegistroB510"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 57): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroB510Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroB510).Assembly);

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
    public void Atributo_DeclaraB510_Nivel3_BlocoB()
    {
        var atributo = typeof(RegistroB510).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("B510");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("B");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroB510Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("B510".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("B510");
        meta.Campos.Select(c => c.Nome).Should().Equal(["IndProf", "IndEsc", "IndSoc", "Cpf", "Nome"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 5));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("B510".AsSpan(), out var meta);
        var registro = (RegistroB510)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());             // IndProf
        meta.Campos[1].Definidor(registro, "0".AsSpan());             // IndEsc
        meta.Campos[2].Definidor(registro, "0".AsSpan());             // IndSoc
        meta.Campos[3].Definidor(registro, "52998224725".AsSpan());   // Cpf
        meta.Campos[4].Definidor(registro, "Maria Souza".AsSpan());   // Nome

        registro.IndProf.Should().Be(IndicadorHabilitacaoProfissional.Habilitado);
        registro.IndEsc.Should().Be(IndicadorEscolaridade.NivelSuperior);
        registro.IndSoc.Should().Be(IndicadorParticipacaoSocietaria.Socio);
        registro.Cpf.Should().Be(Cpf.Criar("52998224725"));
        registro.Nome.Should().Be("Maria Souza");
    }

    [Theory]
    [InlineData(IndicadorHabilitacaoProfissional.Habilitado, "0")]
    [InlineData(IndicadorHabilitacaoProfissional.NaoHabilitado, "1")]
    public void Serializar_IndProf_RetornaCodigo(IndicadorHabilitacaoProfissional habilitacao, string esperado)
    {
        _catalogo.TentarObter("B510".AsSpan(), out var meta);
        var registro = (RegistroB510)meta!.Fabrica();
        registro.IndProf = habilitacao;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorEscolaridade.NivelSuperior, "0")]
    [InlineData(IndicadorEscolaridade.NivelMedio, "1")]
    public void Serializar_IndEsc_RetornaCodigo(IndicadorEscolaridade escolaridade, string esperado)
    {
        _catalogo.TentarObter("B510".AsSpan(), out var meta);
        var registro = (RegistroB510)meta!.Fabrica();
        registro.IndEsc = escolaridade;

        meta.Campos[1].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorParticipacaoSocietaria.Socio, "0")]
    [InlineData(IndicadorParticipacaoSocietaria.NaoSocio, "1")]
    public void Serializar_IndSoc_RetornaCodigo(IndicadorParticipacaoSocietaria participacao, string esperado)
    {
        _catalogo.TentarObter("B510".AsSpan(), out var meta);
        var registro = (RegistroB510)meta!.Fabrica();
        registro.IndSoc = participacao;

        meta.Campos[2].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Sócia habilitada com nível superior.
        const string sped = "|B510|0|0|0|52998224725|Maria Souza|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ProfissionalNaoHabilitadoNivelMedio_PreservaTextoCanonico()
    {
        // Empregado não habilitado, nível médio, não sócio.
        const string sped = "|B510|1|1|1|98765432100|Jose Santos|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
