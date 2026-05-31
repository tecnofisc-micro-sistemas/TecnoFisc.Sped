using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D101 — Informação Complementar dos Documentos Fiscais quando das Prestações
/// Interestaduais Destinadas a Consumidor Final Não Contribuinte EC 87/15 (Códigos 57, 63 e 67).
/// Nível hierárquico 3, ocorrência 1:1.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 168-169.
/// </summary>
[RegistroSped(Codigo = "D101", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD101 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D101";

    /// <summary>Valor total relativo ao Fundo de Combate à Pobreza (FCP) da UF de destino.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlFcpUfDest { get; set; }

    /// <summary>Valor total do ICMS Interestadual para a UF de destino.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcmsUfDest { get; set; }

    /// <summary>Valor total do ICMS Interestadual para a UF do remetente.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcmsUfRem { get; set; }
}
