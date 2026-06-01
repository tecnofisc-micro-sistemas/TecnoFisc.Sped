using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoJ;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoJ;

/// <summary>
/// Sub-stage 10.056 — exercita a forma do <see cref="RegistroJ935"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 207): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroJ935Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

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
    public void Atributo_DeclaraJ935_Nivel3_BlocoJ()
    {
        var atributo = typeof(RegistroJ935).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("J935");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("J");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroJ935Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("J935".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("J935");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NiCpfCnpj", "NomeAuditorFirma", "CodCvmAuditor",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("J935".AsSpan(), out var meta);
        var registro = (RegistroJ935)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12345678910".AsSpan());   // NiCpfCnpj
        meta.Campos[1].Definidor(registro, "AUDITOR TESTE".AsSpan()); // NomeAuditorFirma
        meta.Campos[2].Definidor(registro, "1234567890".AsSpan());    // CodCvmAuditor

        registro.NiCpfCnpj.Should().Be("12345678910");
        registro.NomeAuditorFirma.Should().Be("AUDITOR TESTE");
        registro.CodCvmAuditor.Should().Be("1234567890");
    }

    [Fact]
    public void Definidor_CampoOpcionalVazio_DevolveNulo()
    {
        _catalogo.TentarObter("J935".AsSpan(), out var meta);
        var registro = (RegistroJ935)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCvmAuditor

        registro.CodCvmAuditor.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ExemploDoManual_PessoaFisica_PreservaTextoCanonico()
    {
        // Exemplo do manual p. 207 — auditor pessoa física com registro CVM
        const string sped = "|J935|12345678910|AUDITOR TESTE|1234567890|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_PessoaJuridica_SemCodCvm_PreservaTextoCanonico()
    {
        // Pessoa jurídica de auditoria — COD_CVM_AUDITOR opcional vazio
        const string sped = "|J935|98765432000195|FIRMA AUDITORIA INDEPENDENTE S.A.||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
