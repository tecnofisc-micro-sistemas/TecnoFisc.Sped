using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Endereço de emitente (<c>enderEmit</c>) ou destinatário (<c>enderDest</c>). A estrutura é
/// idêntica nos dois grupos do leiaute 4.00, por isso um único tipo serve a ambos.
/// </summary>
public sealed record Endereco
{
    /// <summary><c>xLgr</c> — logradouro.</summary>
    public required string XLgr { get; init; }

    /// <summary><c>nro</c> — número.</summary>
    public required string Nro { get; init; }

    /// <summary><c>xCpl</c> — complemento (opcional).</summary>
    public string? XCpl { get; init; }

    /// <summary><c>xBairro</c> — bairro.</summary>
    public required string XBairro { get; init; }

    /// <summary><c>cMun</c> — código IBGE do município.</summary>
    public required CodigoMunicipioIbge CMun { get; init; }

    /// <summary><c>xMun</c> — nome do município.</summary>
    public required string XMun { get; init; }

    /// <summary><c>UF</c> — sigla da unidade federativa.</summary>
    public required string UF { get; init; }

    /// <summary><c>CEP</c> — código de endereçamento postal (opcional).</summary>
    public string? CEP { get; init; }

    /// <summary><c>cPais</c> — código do país (opcional, 1058 = Brasil).</summary>
    public int? CPais { get; init; }

    /// <summary><c>xPais</c> — nome do país (opcional).</summary>
    public string? XPais { get; init; }

    /// <summary><c>fone</c> — telefone (opcional).</summary>
    public string? Fone { get; init; }
}
