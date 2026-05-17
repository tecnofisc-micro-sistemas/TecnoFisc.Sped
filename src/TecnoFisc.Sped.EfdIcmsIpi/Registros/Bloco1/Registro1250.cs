using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1250 - Informacoes Consolidadas de Saldos de Restituicao, Ressarcimento e
/// Complementacao do ICMS. Nivel hierarquico 2, ocorrencia unica por arquivo. Conforme
/// Guia Pratico EFD-ICMS/IPI V3.0.6, p. 274.
/// </summary>
[RegistroSped(Codigo = "1250", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1250 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1250";

    /// <summary>Valor total do ICMS operacao propria que o informante tem direito ao credito.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCreditoIcmsOp { get; set; }

    /// <summary>Valor total do ICMS ST que o informante tem direito ao credito.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcmsStRest { get; set; }

    /// <summary>Valor total do FCP_ST agregado ao valor do ICMS ST restituido.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlFcpStRest { get; set; }

    /// <summary>Valor total do debito referente ao complemento do imposto.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcmsStCompl { get; set; }

    /// <summary>Valor total do FCP_ST agregado ao valor do ICMS ST complementado.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlFcpStCompl { get; set; }
}
