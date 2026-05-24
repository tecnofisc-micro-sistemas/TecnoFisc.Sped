using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E210 — Apuração do ICMS — Substituição Tributária.
/// Nível hierárquico 3, ocorrência um por período. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 214.
/// </summary>
[RegistroSped(Codigo = "E210", Nivel = 3, Bloco = "E")]
public sealed partial class RegistroE210 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E210";

    /// <summary>Indicador de movimento de ST — 0 sem operações, 1 com operações.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoSt IndMovSt { get; set; }

    /// <summary>Valor do saldo credor de período anterior de Substituição Tributária.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldCredAntSt { get; set; }

    /// <summary>Valor total do ICMS ST de devolução de mercadorias.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDevolSt { get; set; }

    /// <summary>Valor total do ICMS ST de ressarcimentos.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRessarcSt { get; set; }

    /// <summary>Valor total de ajustes "Outros créditos ST" e "Estorno de débitos ST".</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOutCredSt { get; set; }

    /// <summary>Valor total dos ajustes a crédito de ICMS ST provenientes de ajustes do documento fiscal.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjCreditosSt { get; set; }

    /// <summary>Valor total do ICMS retido por Substituição Tributária.</summary>
    /// <remarks>
    /// <b>V017 (Guide 3.1.4 item 7):</b> nova validação no campo 08 <c>VL_RETENCAO_ST</c>.
    /// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
    /// </remarks>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRetencaoSt { get; set; }

    /// <summary>Valor total dos ajustes "Outros débitos ST" e "Estorno de créditos ST".</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOutDebSt { get; set; }

    /// <summary>Valor total dos ajustes a débito de ICMS ST provenientes de ajustes do documento fiscal.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjDebitosSt { get; set; }

    /// <summary>Valor total de saldo devedor antes das deduções.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldDevAntSt { get; set; }

    /// <summary>Valor total dos ajustes "Deduções ST".</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDeducoesSt { get; set; }

    /// <summary>Imposto a recolher ST (campo 11 menos campo 12).</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcmsRecolSt { get; set; }

    /// <summary>Saldo credor de ST a transportar para o período seguinte.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldCredStTransportar { get; set; }

    /// <summary>Valores recolhidos ou a recolher, extra-apuração.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal DebEspSt { get; set; }
}
