using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoJ;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoJ;

/// <summary>
/// Sub-stage 10.055 — exercita a forma do <see cref="RegistroJ932"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 203–206): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroJ932Tests
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
    public void Atributo_DeclaraJ932_Nivel3_BlocoJ()
    {
        var atributo = typeof(RegistroJ932).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("J932");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("J");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroJ932Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("J932".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("J932");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IdentNomT", "IdentCpfCnpjT", "IdentQualifT", "CodAssinT",
            "IndCrcT", "EmailT", "FoneT", "UfCrcT", "NumSeqCrcT", "DtCrcT",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("J932".AsSpan(), out var meta);
        var registro = (RegistroJ932)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "FULANO BELTRANO".AsSpan());                                                         // IdentNomT
        meta.Campos[1].Definidor(registro, "12345678900".AsSpan());                                                              // IdentCpfCnpjT
        meta.Campos[2].Definidor(registro, "CONTADOR/CONTABILISTA RESPONSAVEL PELO TERMO".AsSpan());                            // IdentQualifT
        meta.Campos[3].Definidor(registro, "910".AsSpan());                                                                      // CodAssinT
        meta.Campos[4].Definidor(registro, "1SP123456".AsSpan());                                                                // IndCrcT
        meta.Campos[5].Definidor(registro, "FULANO@GMAIL.COM".AsSpan());                                                         // EmailT
        meta.Campos[6].Definidor(registro, "2199999999".AsSpan());                                                               // FoneT
        meta.Campos[7].Definidor(registro, "RJ".AsSpan());                                                                       // UfCrcT
        meta.Campos[8].Definidor(registro, "RJ/2012/001".AsSpan());                                                              // NumSeqCrcT
        meta.Campos[9].Definidor(registro, "31122023".AsSpan());                                                                  // DtCrcT

        registro.IdentNomT.Should().Be("FULANO BELTRANO");
        registro.IdentCpfCnpjT.Should().Be("12345678900");
        registro.IdentQualifT.Should().Be("CONTADOR/CONTABILISTA RESPONSAVEL PELO TERMO");
        registro.CodAssinT.Should().Be(CodigoQualificacaoAssinante.ContadorTermoSubstituicao);
        registro.IndCrcT.Should().Be("1SP123456");
        registro.EmailT.Should().Be("FULANO@GMAIL.COM");
        registro.FoneT.Should().Be("2199999999");
        registro.UfCrcT.Should().Be("RJ");
        registro.NumSeqCrcT.Should().Be("RJ/2012/001");
        registro.DtCrcT.Should().Be(new DateOnly(2023, 12, 31));
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("J932".AsSpan(), out var meta);
        var registro = (RegistroJ932)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // IndCrcT
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // EmailT
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);  // FoneT
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // UfCrcT
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // NumSeqCrcT
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // DtCrcT

        registro.IndCrcT.Should().BeNull();
        registro.EmailT.Should().BeNull();
        registro.FoneT.Should().BeNull();
        registro.UfCrcT.Should().BeNull();
        registro.NumSeqCrcT.Should().BeNull();
        registro.DtCrcT.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ExemploDoManual_ContadorTermo_PreservaTextoCanonico()
    {
        // Exemplo do manual p. 206 — contador responsável pelo termo com dados CRC completos
        const string sped = "|J932|FULANO BELTRANO|12345678900|CONTADOR/CONTABILISTA RESPONSÁVEL PELO TERMO DE VERIFICAÇÃO PARA FINS DE SUBSTITUIÇÃO DA ECD|910|1SP123456|FULANO@GMAIL.COM|2199999999|RJ|RJ/2012/001|31122023|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AuditorIndependente_SemDadosCrc_PreservaTextoCanonico()
    {
        // Auditor independente (código 920) — campos de CRC opcionais vazios
        const string sped = "|J932|FIRMA AUDITORIA S.A.|98765432000195|AUDITOR INDEPENDENTE RESPONSÁVEL PELO TERMO|920|||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
