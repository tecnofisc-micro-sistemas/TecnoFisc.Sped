using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C140 — Complemento — Fatura (cód. 01).
/// Nível hierárquico 3, ocorrência 1:1.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 73-74.
/// </summary>
[RegistroSped(Codigo = "C140", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC140 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C140";

    /// <summary>Indicador do emitente do título: 0-Emissão própria; 1-Terceiros.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEmissorDocumento IndEmit { get; set; }

    /// <summary>Indicador do tipo de título de crédito: 00-Duplicata; 01-Cheque; 02-Promissória; 03-Recibo; 99-Outros.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public IndicadorTipoTitulo IndTit { get; set; }

    /// <summary>Descrição complementar do título de crédito. Obrigatório quando IND_TIT = 99.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? DescTit { get; set; }

    /// <summary>Número ou código identificador do título de crédito.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public string? NumTit { get; set; }

    /// <summary>Quantidade de parcelas a receber/pagar. Deve corresponder ao total de ocorrências do Registro C141.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Obrigatorio = true)]
    public int QtdParc { get; set; }

    /// <summary>Valor total dos títulos de créditos.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTit { get; set; }
}
