using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C113 — Documento Fiscal Referenciado.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 69.
/// </summary>
[RegistroSped(Codigo = "C113", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC113 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C113";

    /// <summary>Indicador do tipo de operação: 0-Entrada/aquisição; 1-Saída/prestação.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacao IndOper { get; set; }

    /// <summary>Indicador do emitente do título: 0-Emissão própria; 1-Terceiros.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEmissorDocumento IndEmit { get; set; }

    /// <summary>Código do participante emitente (campo 02 do Registro 0150) do documento referenciado.</summary>
    [CampoSped(Ordem = 4, Tamanho = 60, Obrigatorio = true)]
    public string? CodPart { get; set; }

    /// <summary>Código do documento fiscal conforme Tabela 4.1.1.</summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 6, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal.</summary>
    [CampoSped(Ordem = 7, Tamanho = 3)]
    public int? Sub { get; set; }

    /// <summary>Número do documento fiscal.</summary>
    [CampoSped(Ordem = 8, Tamanho = 9, Obrigatorio = true)]
    public int? NumDoc { get; set; }

    /// <summary>Data da emissão do documento fiscal no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 9, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Chave do Documento Eletrônico (NF-e, CT-e ou CT-e OS).</summary>
    [CampoSped(Ordem = 10, Tamanho = 44)]
    public ChaveAcesso? ChvDoce { get; set; }
}
