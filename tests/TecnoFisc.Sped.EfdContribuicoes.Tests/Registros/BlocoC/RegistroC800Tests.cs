using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC800Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC800).Assembly);

    // Chave CF-e válida: UF(2)+AAMM(4)+CNPJ(14)+Modelo(2)+NúmSérie(9)+NumCFe(6)+CodNum(6)+DV(1) = 44 chars.
    private const string ChaveCfeValida = "35240159900001234000590000000010000011234567";

    [Fact]
    public void Atributo_DeclaraC800_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC800).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C800");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC800ComDezesseisCamposNaOrdem()
    {
        _catalogo.TentarObter("C800".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C800");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodMod", "CodSit", "NumCfe", "DtDoc", "VlCfe",
            "VlPis", "VlCofins", "CnpjCpf", "NrSat", "ChvCfe",
            "VlDesc", "VlMerc", "VlOutDa", "VlIcms", "VlPisSt",
            "VlCofinsSt",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // CodMod
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // CodSit
        meta.Campos[2].Tamanho.Should().Be(9);
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // NumCfe
        meta.Campos[3].Tamanho.Should().Be(8);
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // DtDoc
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // VlCfe
        meta.Campos[5].Obrigatorio.Should().BeFalse();  // VlPis
        meta.Campos[8].Tamanho.Should().Be(9);
        meta.Campos[8].Obrigatorio.Should().BeFalse();  // NrSat
        meta.Campos[9].Tamanho.Should().Be(44);
        meta.Campos[9].Obrigatorio.Should().BeFalse();  // ChvCfe
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C800".AsSpan(), out var meta);
        var registro = (RegistroC800)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "59".AsSpan());                 // CodMod
        meta.Campos[1].Definidor(registro, "00".AsSpan());                 // CodSit
        meta.Campos[2].Definidor(registro, "123456789".AsSpan());          // NumCfe
        meta.Campos[3].Definidor(registro, "15012024".AsSpan());           // DtDoc
        meta.Campos[4].Definidor(registro, "1000,00".AsSpan());            // VlCfe
        meta.Campos[5].Definidor(registro, "6,50".AsSpan());               // VlPis
        meta.Campos[6].Definidor(registro, "30,00".AsSpan());              // VlCofins
        meta.Campos[7].Definidor(registro, "12345678000195".AsSpan());     // CnpjCpf
        meta.Campos[8].Definidor(registro, "900001234".AsSpan());          // NrSat
        meta.Campos[9].Definidor(registro, ChaveCfeValida.AsSpan());       // ChvCfe
        meta.Campos[10].Definidor(registro, "50,00".AsSpan());             // VlDesc
        meta.Campos[11].Definidor(registro, "920,00".AsSpan());            // VlMerc
        meta.Campos[12].Definidor(registro, "30,00".AsSpan());             // VlOutDa
        meta.Campos[13].Definidor(registro, "120,00".AsSpan());            // VlIcms
        meta.Campos[14].Definidor(registro, "3,25".AsSpan());              // VlPisSt
        meta.Campos[15].Definidor(registro, "15,00".AsSpan());             // VlCofinsSt

        registro.CodMod.Should().Be("59");
        registro.CodSit.Should().Be(CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.NumCfe.Should().Be(123456789);
        registro.DtDoc.Should().Be(new DateOnly(2024, 1, 15));
        registro.VlCfe.Should().Be(1000.00m);
        registro.VlPis.Should().Be(6.50m);
        registro.VlCofins.Should().Be(30.00m);
        registro.CnpjCpf.Should().Be("12345678000195");
        registro.NrSat.Should().Be(900001234);
        registro.ChvCfe.Should().Be(ChaveCfeValida);
        registro.VlDesc.Should().Be(50.00m);
        registro.VlMerc.Should().Be(920.00m);
        registro.VlOutDa.Should().Be(30.00m);
        registro.VlIcms.Should().Be(120.00m);
        registro.VlPisSt.Should().Be(3.25m);
        registro.VlCofinsSt.Should().Be(15.00m);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C800".AsSpan(), out var meta);
        var registro = (RegistroC800)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlPis
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlCofins
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);   // CnpjCpf
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);   // NrSat
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);   // ChvCfe
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlDesc
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlMerc
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlOutDa
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlIcms
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlPisSt
        meta.Campos[15].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCofinsSt

        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CnpjCpf.Should().BeNull();
        registro.NrSat.Should().BeNull();
        registro.ChvCfe.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.VlMerc.Should().BeNull();
        registro.VlOutDa.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlPisSt.Should().BeNull();
        registro.VlCofinsSt.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            $"|C800|59|00|123456789|15012024|1000,00|6,50|30,00|12345678000195|900001234|{ChaveCfeValida}|50,00|920,00|30,00|120,00|3,25|15,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposObrigatoriosApenas_PreservaTextoCanonico()
    {
        const string sped = "|C800|59|00|123456789|15012024|1000,00||||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DocumentoCancelado_PreservaTextoCanonico()
    {
        const string sped = "|C800|59|02|1|01012024|500,00||||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
