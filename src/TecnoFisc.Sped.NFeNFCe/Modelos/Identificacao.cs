using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.NFeNFCe.Enums;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Grupo <c>ide</c> — identificação da NF-e/NFC-e (leiaute 4.00).
/// </summary>
public sealed record Identificacao
{
    /// <summary><c>cUF</c> — código IBGE da UF do emitente.</summary>
    public required int CUF { get; init; }

    /// <summary><c>cNF</c> — código numérico aleatório que compõe a chave de acesso (8 dígitos).</summary>
    public required string CNF { get; init; }

    /// <summary><c>natOp</c> — descrição da natureza da operação.</summary>
    public required string NatOp { get; init; }

    /// <summary><c>mod</c> — modelo do documento (55 NF-e, 65 NFC-e).</summary>
    public required ModeloDocumento Mod { get; init; }

    /// <summary><c>serie</c> — série do documento.</summary>
    public required int Serie { get; init; }

    /// <summary><c>nNF</c> — número do documento.</summary>
    public required int NNF { get; init; }

    /// <summary><c>dhEmi</c> — data e hora de emissão (com offset de fuso).</summary>
    public required DateTimeOffset DhEmi { get; init; }

    /// <summary><c>dhSaiEnt</c> — data e hora de saída/entrada (opcional).</summary>
    public DateTimeOffset? DhSaiEnt { get; init; }

    /// <summary><c>tpNF</c> — tipo da operação: 0 entrada, 1 saída.</summary>
    public required IndicadorOperacao TpNF { get; init; }

    /// <summary><c>idDest</c> — local de destino: 1 interna, 2 interestadual, 3 exterior.</summary>
    public required int IdDest { get; init; }

    /// <summary><c>cMunFG</c> — código IBGE do município de ocorrência do fato gerador.</summary>
    public required CodigoMunicipioIbge CMunFG { get; init; }

    /// <summary><c>tpImp</c> — formato de impressão do DANFE.</summary>
    public int TpImp { get; init; }

    /// <summary><c>tpEmis</c> — forma de emissão.</summary>
    public required TipoEmissao TpEmis { get; init; }

    /// <summary><c>cDV</c> — dígito verificador da chave de acesso.</summary>
    public int CDV { get; init; }

    /// <summary><c>tpAmb</c> — ambiente: produção ou homologação.</summary>
    public required TipoAmbiente TpAmb { get; init; }

    /// <summary><c>finNFe</c> — finalidade da emissão.</summary>
    public required FinalidadeEmissao FinNFe { get; init; }

    /// <summary><c>indFinal</c> — operação com consumidor final (0 não, 1 sim).</summary>
    public bool IndFinal { get; init; }

    /// <summary><c>indPres</c> — indicador de presença do comprador.</summary>
    public required IndicadorPresenca IndPres { get; init; }

    /// <summary><c>indIntermed</c> — indicador de intermediador/marketplace (opcional).</summary>
    public IndicadorIntermediador? IndIntermed { get; init; }

    /// <summary><c>procEmi</c> — processo de emissão.</summary>
    public int ProcEmi { get; init; }

    /// <summary><c>verProc</c> — versão do aplicativo emissor.</summary>
    public string? VerProc { get; init; }
}
