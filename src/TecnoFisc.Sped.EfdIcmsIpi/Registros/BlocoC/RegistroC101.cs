using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C101 — Informação Complementar dos Documentos Fiscais quando das Operações Interestaduais
/// Destinadas a Consumidor Final Não Contribuinte EC 87/15 (Código 55).
/// Nível hierárquico 3, ocorrência 1:1.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 65.
/// </summary>
[RegistroSped(Codigo = "C101", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC101 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C101";

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
