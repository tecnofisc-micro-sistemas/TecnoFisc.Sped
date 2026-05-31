using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1255 - Informacoes Consolidadas de Saldos de Restituicao, Ressarcimento e
/// Complementacao do ICMS por Motivo. Nivel hierarquico 3, ocorrencia varios por Registro
/// 1250. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 274-275.
/// </summary>
[RegistroSped(Codigo = "1255", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1255 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1255";

    /// <summary>Codigo do motivo da restituicao ou complementacao conforme Tabela 5.7 da UF.</summary>
    [CampoSped(Ordem = 2, Tamanho = 5, Obrigatorio = true)]
    public string? CodMotRestCompl { get; set; }

    /// <summary>Valor total do ICMS operacao propria com direito ao credito para o mesmo motivo.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCreditoIcmsOpMot { get; set; }

    /// <summary>Valor total do ICMS ST com direito ao credito para o mesmo motivo.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcmsStRestMot { get; set; }

    /// <summary>Valor total do FCP_ST agregado ao ICMS ST restituido para o mesmo motivo.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlFcpStRestMot { get; set; }

    /// <summary>Valor total do debito referente ao complemento do imposto para o mesmo motivo.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcmsStComplMot { get; set; }

    /// <summary>Valor total do FCP_ST agregado ao ICMS ST complementado para o mesmo motivo.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlFcpStComplMot { get; set; }
}
