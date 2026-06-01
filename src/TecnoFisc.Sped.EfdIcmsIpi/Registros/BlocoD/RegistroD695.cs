using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D695 — Consolidação da Prestação de Serviços — NF Serviço de Comunicação (cód. 21)
/// e Serviço de Telecomunicação (cód. 22), para empresas obrigadas ao Convênio ICMS 115/2003.
/// Nível hierárquico 2, ocorrência vários por arquivo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, pp. 202-203.
/// </summary>
[RegistroSped(Codigo = "D695", Nivel = 2, Bloco = "D")]
public sealed partial class RegistroD695 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D695";

    /// <summary>Código do modelo do documento fiscal, conforme a Tabela 4.1.1 (21 ou 22).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2)]
    public string? CodMod { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Número de ordem inicial dos documentos consolidados.</summary>
    [CampoSped(Ordem = 4, Tamanho = 9)]
    public int? NroOrdIni { get; set; }

    /// <summary>Número de ordem final dos documentos consolidados.</summary>
    [CampoSped(Ordem = 5, Tamanho = 9)]
    public int? NroOrdFin { get; set; }

    /// <summary>Data de emissão inicial dos documentos / data inicial de vencimento da fatura, no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 6, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtDocIni { get; set; }

    /// <summary>Data de emissão final dos documentos / data final de vencimento da fatura, no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtDocFin { get; set; }

    /// <summary>Nome do arquivo Mestre de Documento Fiscal (conforme Convênio ICMS 115/2003).</summary>
    [CampoSped(Ordem = 8, Tamanho = 33)]
    public string? NomMest { get; set; }

    /// <summary>Chave de codificação digital do arquivo Mestre de Documento Fiscal.</summary>
    [CampoSped(Ordem = 9, Tamanho = 32)]
    public string? ChvCodDig { get; set; }
}
