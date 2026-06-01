using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C112 — Documento de Arrecadação Referenciado.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 68.
/// </summary>
[RegistroSped(Codigo = "C112", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC112 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C112";

    /// <summary>Código do modelo do documento de arrecadação: 0-Documento estadual; 1-GNRE.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public CodigoModeloDocumentoArrecadacao CodDa { get; set; }

    /// <summary>Sigla da unidade federada beneficiária do recolhimento.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? Uf { get; set; }

    /// <summary>Número do documento de arrecadação.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? NumDa { get; set; }

    /// <summary>Código completo da autenticação bancária.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? CodAut { get; set; }

    /// <summary>Valor total do documento de arrecadação (principal, atualização monetária, juros e multa).</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDa { get; set; }

    /// <summary>Data de vencimento do documento de arrecadação no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtVcto { get; set; }

    /// <summary>Data de pagamento do documento de arrecadação no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 8, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtPgto { get; set; }
}
