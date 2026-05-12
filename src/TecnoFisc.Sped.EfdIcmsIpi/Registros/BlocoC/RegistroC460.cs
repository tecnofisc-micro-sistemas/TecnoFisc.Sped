using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C460 — Documento Fiscal Emitido por ECF (código 02, 2D e 60).
/// Identifica os documentos fiscais emitidos pelos usuários de equipamentos ECF,
/// totalizados na Redução Z.
/// Nível hierárquico 4, ocorrência 1:N por C405.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 125.
/// </summary>
[RegistroSped(Codigo = "C460", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC460 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C460";

    /// <summary>Código do modelo do documento fiscal, conforme Tabela 4.1.1. Valores válidos: 02, 2D, 60.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código da situação do documento fiscal, conforme Tabela 4.1.2. Valores válidos para ECF: 00, 01, 02.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal CodSit { get; set; }

    /// <summary>Número do documento fiscal (COO — Contador de Ordem de Operação).</summary>
    [CampoSped(Ordem = 4, Tamanho = 9, Obrigatorio = true)]
    public int? NumDoc { get; set; }

    /// <summary>Data da emissão do documento fiscal, no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 5, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Valor total do documento fiscal.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    /// <summary>Valor do PIS. Dispensado para contribuintes que entregam EFD-Contribuições relativa ao mesmo período.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor da COFINS. Dispensado para contribuintes que entregam EFD-Contribuições relativa ao mesmo período.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>CPF (11 dígitos) ou CNPJ (14 dígitos) do adquirente.</summary>
    [CampoSped(Ordem = 9, Tamanho = 14)]
    public string? CpfCnpj { get; set; }

    /// <summary>Nome do adquirente.</summary>
    [CampoSped(Ordem = 10, Tamanho = 60)]
    public string? NomAdq { get; set; }
}
