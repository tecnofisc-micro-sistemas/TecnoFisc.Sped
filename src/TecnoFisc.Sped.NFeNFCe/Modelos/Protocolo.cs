using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Grupo <c>protNFe/infProt</c> — protocolo de autorização da NF-e/NFC-e. Presente quando a
/// origem é um <c>nfeProc</c> (nota processada pela SEFAZ).
/// </summary>
public sealed record Protocolo
{
    /// <summary><c>tpAmb</c> — ambiente em que a nota foi autorizada.</summary>
    public required TipoAmbiente TpAmb { get; init; }

    /// <summary><c>verAplic</c> — versão do aplicativo que processou a nota (opcional).</summary>
    public string? VerAplic { get; init; }

    /// <summary><c>chNFe</c> — chave de acesso da nota autorizada.</summary>
    public required ChaveAcesso ChNFe { get; init; }

    /// <summary><c>dhRecbto</c> — data e hora do processamento.</summary>
    public required DateTimeOffset DhRecbto { get; init; }

    /// <summary><c>nProt</c> — número do protocolo de autorização (opcional).</summary>
    public string? NProt { get; init; }

    /// <summary><c>digVal</c> — digest value da assinatura (opcional).</summary>
    public string? DigVal { get; init; }

    /// <summary><c>cStat</c> — código de status da resposta (100 = autorizado, 150 = autorizado fora do prazo).</summary>
    public required int CStat { get; init; }

    /// <summary><c>xMotivo</c> — descrição do status (opcional).</summary>
    public string? XMotivo { get; init; }

    /// <summary>Verdadeiro quando o status indica autorização de uso (100 ou 150).</summary>
    public bool IsAutorizada => CStat is 100 or 150;
}
