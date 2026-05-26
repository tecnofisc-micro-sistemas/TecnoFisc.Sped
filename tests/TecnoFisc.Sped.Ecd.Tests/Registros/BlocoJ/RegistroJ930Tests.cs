using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoJ;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoJ;

/// <summary>
/// Sub-stage 10.054 — exercita a forma do <see cref="RegistroJ930"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 197–203): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroJ930Tests
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
    public void Atributo_DeclaraJ930_Nivel3_BlocoJ()
    {
        var atributo = typeof(RegistroJ930).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("J930");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("J");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroJ930Com11CamposNaOrdem()
    {
        _catalogo.TentarObter("J930".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("J930");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IdentNom", "IdentCpfCnpj", "IdentQualif", "CodAssin",
            "IndCrc", "Email", "Fone", "UfCrc", "NumSeqCrc", "DtCrc", "IndRespLegal",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("J930".AsSpan(), out var meta);
        var registro = (RegistroJ930)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "FULANO BELTRANO".AsSpan());    // IdentNom
        meta.Campos[1].Definidor(registro, "12345678900".AsSpan());         // IdentCpfCnpj
        meta.Campos[2].Definidor(registro, "CONTADOR".AsSpan());            // IdentQualif
        meta.Campos[3].Definidor(registro, "900".AsSpan());                 // CodAssin
        meta.Campos[4].Definidor(registro, "1SP123456".AsSpan());           // IndCrc
        meta.Campos[5].Definidor(registro, "FULANO@GMAIL.COM".AsSpan());    // Email
        meta.Campos[6].Definidor(registro, "2199999999".AsSpan());          // Fone
        meta.Campos[7].Definidor(registro, "RJ".AsSpan());                  // UfCrc
        meta.Campos[8].Definidor(registro, "RJ/2012/001".AsSpan());         // NumSeqCrc
        meta.Campos[9].Definidor(registro, "31122023".AsSpan());            // DtCrc
        meta.Campos[10].Definidor(registro, "S".AsSpan());                  // IndRespLegal

        registro.IdentNom.Should().Be("FULANO BELTRANO");
        registro.IdentCpfCnpj.Should().Be("12345678900");
        registro.IdentQualif.Should().Be("CONTADOR");
        registro.CodAssin.Should().Be(CodigoQualificacaoAssinante.ContadorContabilista);
        registro.IndCrc.Should().Be("1SP123456");
        registro.Email.Should().Be("FULANO@GMAIL.COM");
        registro.Fone.Should().Be("2199999999");
        registro.UfCrc.Should().Be("RJ");
        registro.NumSeqCrc.Should().Be("RJ/2012/001");
        registro.DtCrc.Should().Be(new DateOnly(2023, 12, 31));
        registro.IndRespLegal.Should().Be(IndicadorSimNao.Sim);
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("J930".AsSpan(), out var meta);
        var registro = (RegistroJ930)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // IndCrc
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // Email
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);  // Fone
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // UfCrc
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // NumSeqCrc
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // DtCrc

        registro.IndCrc.Should().BeNull();
        registro.Email.Should().BeNull();
        registro.Fone.Should().BeNull();
        registro.UfCrc.Should().BeNull();
        registro.NumSeqCrc.Should().BeNull();
        registro.DtCrc.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ExemploDoManual_PreservaTextoCanonico()
    {
        // Exemplo do manual p. 203 — contador responsável legal com dados CRC completos
        const string sped = "|J930|FULANO BELTRANO|12345678900|CONTADOR|900|1SP123456|FULANO@GMAIL.COM|2199999999|RJ|RJ/2012/001|31122023|S|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DiretorSemDadosCrc_PreservaTextoCanonico()
    {
        // Diretor como responsável legal; campos opcionais de CRC vazios
        const string sped = "|J930|EMPRESA BETA LTDA|98765432000195|DIRETOR|203|||||||N|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
