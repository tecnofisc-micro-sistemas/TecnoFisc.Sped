using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.052 — exercita a forma do <see cref="RegistroC165"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 75-76): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC165Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC165).Assembly);

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
    public void Atributo_DeclaraC165_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC165).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C165");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC165Com12CamposNaOrdem()
    {
        _catalogo.TentarObter("C165".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C165");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodPart", "VeicId", "CodAut", "NrPasse", "Hora",
            "Temper", "QtdVol", "PesoBrt", "PesoLiq", "NomMot", "Cpf", "UfId",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 12));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C165".AsSpan(), out var meta);
        var registro = (RegistroC165)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "TRANSP001".AsSpan());   // CodPart
        meta.Campos[1].Definidor(registro, "ABC1234".AsSpan());     // VeicId
        meta.Campos[2].Definidor(registro, "AUT123".AsSpan());      // CodAut
        meta.Campos[3].Definidor(registro, "PASSE99".AsSpan());     // NrPasse
        meta.Campos[4].Definidor(registro, "140000".AsSpan());      // Hora
        meta.Campos[5].Definidor(registro, "25,5".AsSpan());        // Temper
        meta.Campos[6].Definidor(registro, "3".AsSpan());           // QtdVol
        meta.Campos[7].Definidor(registro, "3000,00".AsSpan());     // PesoBrt
        meta.Campos[8].Definidor(registro, "2800,50".AsSpan());     // PesoLiq
        meta.Campos[9].Definidor(registro, "João Silva".AsSpan());  // NomMot
        meta.Campos[10].Definidor(registro, "12345678909".AsSpan()); // Cpf
        meta.Campos[11].Definidor(registro, "SP".AsSpan());         // UfId

        registro.CodPart.Should().Be("TRANSP001");
        registro.VeicId.Should().Be("ABC1234");
        registro.CodAut.Should().Be("AUT123");
        registro.NrPasse.Should().Be("PASSE99");
        registro.Hora.Should().Be("140000");
        registro.Temper.Should().Be(25.5m);
        registro.QtdVol.Should().Be(3);
        registro.PesoBrt.Should().Be(3000.00m);
        registro.PesoLiq.Should().Be(2800.50m);
        registro.NomMot.Should().Be("João Silva");
        registro.Cpf.Should().Be(Cpf.Create("12345678909"));
        registro.UfId.Should().Be("SP");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C165".AsSpan(), out var meta);
        var registro = (RegistroC165)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodPart.Should().BeNull();
        registro.VeicId.Should().BeNull();
        registro.CodAut.Should().BeNull();
        registro.NrPasse.Should().BeNull();
        registro.Hora.Should().BeNull();
        registro.Temper.Should().BeNull();
        registro.QtdVol.Should().BeNull();
        registro.PesoBrt.Should().BeNull();
        registro.PesoLiq.Should().BeNull();
        registro.NomMot.Should().BeNull();
        registro.Cpf.Should().BeNull();
        registro.UfId.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Operação com combustível com todos os campos preenchidos.
        const string sped = "|C165|TRANSP001|ABC1234|AUT123|PASSE99|140000|25,5|3|3000,00|2800,50|João Silva|12345678909|SP|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Apenas campos de saída obrigatórios preenchidos; opcionais vazios.
        const string sped = "|C165||ABC1234|||140000||5|5000,00|4500,00||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
