using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D180 — Modais (Código 26).
/// Identifica transportadores e seus documentos fiscais emitidos durante o transporte multimodal.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 176-177.
/// </summary>
[RegistroSped(Codigo = "D180", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD180 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D180";

    /// <summary>Número de ordem sequencial do modal.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0)]
    public int? NumSeq { get; set; }

    /// <summary>Indicador do emitente do documento fiscal: 0 - Emissão própria; 1 - Terceiros.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1)]
    public IndicadorEmissorDocumento? IndEmit { get; set; }

    /// <summary>CNPJ ou CPF do participante emitente do modal (14 dígitos para CNPJ, 11 para CPF).</summary>
    [CampoSped(Ordem = 4, Tamanho = 14)]
    public string? CnpjCpfEmit { get; set; }

    /// <summary>Sigla da unidade da federação do participante emitente do modal.</summary>
    [CampoSped(Ordem = 5, Tamanho = 2)]
    public string? UfEmit { get; set; }

    /// <summary>Inscrição Estadual do participante emitente do modal.</summary>
    [CampoSped(Ordem = 6, Tamanho = 14)]
    public string? IeEmit { get; set; }

    /// <summary>Código do município de origem do serviço, conforme tabela IBGE. Preencher com 9999999 se Exterior.</summary>
    [CampoSped(Ordem = 7, Tamanho = 7)]
    public int? CodMunOrig { get; set; }

    /// <summary>CNPJ ou CPF do participante tomador do serviço (14 dígitos para CNPJ, 11 para CPF).</summary>
    [CampoSped(Ordem = 8, Tamanho = 14)]
    public string? CnpjCpfTom { get; set; }

    /// <summary>Sigla da unidade da federação do participante tomador do serviço.</summary>
    [CampoSped(Ordem = 9, Tamanho = 2)]
    public string? UfTom { get; set; }

    /// <summary>Inscrição Estadual do participante tomador do serviço.</summary>
    [CampoSped(Ordem = 10, Tamanho = 14)]
    public string? IeTom { get; set; }

    /// <summary>Código do município de destino, conforme tabela IBGE. Preencher com 9999999 se Exterior.</summary>
    [CampoSped(Ordem = 11, Tamanho = 7)]
    public int? CodMunDest { get; set; }

    /// <summary>Código do modelo do documento fiscal, conforme a Tabela 4.1.1.</summary>
    [CampoSped(Ordem = 12, Tamanho = 2)]
    public ModeloDocumento? CodMod { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 13, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal.</summary>
    [CampoSped(Ordem = 14, Tamanho = 3)]
    public string? Sub { get; set; }

    /// <summary>Número do documento fiscal.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0)]
    public int? NumDoc { get; set; }

    /// <summary>Data da emissão do documento fiscal no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 16, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtDoc { get; set; }

    /// <summary>Valor total do documento fiscal.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2)]
    public decimal? VlDoc { get; set; }
}
