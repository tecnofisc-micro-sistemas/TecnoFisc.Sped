using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoG;

/// <summary>
/// Registro G130 - Identificacao do documento fiscal.
/// Nivel hierarquico 4, ocorrencia varios por Registro G125. Conforme Guia Pratico
/// EFD-ICMS/IPI V3.0.6, p. 242-243.
/// </summary>
[RegistroSped(Codigo = "G130", Nivel = 4, Bloco = "G")]
public sealed partial class RegistroG130 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "G130";

    /// <summary>Indicador do emitente do documento fiscal.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEmissorDocumento IndEmit { get; set; }

    /// <summary>Codigo do participante do documento fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodPart { get; set; }

    /// <summary>Codigo do modelo de documento fiscal, conforme Tabela 4.1.1.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true)]
    public ModeloDocumento CodMod { get; set; }

    /// <summary>Serie do documento fiscal.</summary>
    [CampoSped(Ordem = 5, Tamanho = 3)]
    public string? Serie { get; set; }

    /// <summary>Numero do documento fiscal.</summary>
    [CampoSped(Ordem = 6, Tamanho = 9, Obrigatorio = true)]
    public int? NumDoc { get; set; }

    /// <summary>Chave do documento fiscal eletronico.</summary>
    [CampoSped(Ordem = 7, Tamanho = 44)]
    public ChaveAcesso? ChvNfeCte { get; set; }

    /// <summary>Data da emissao do documento fiscal (ddMMyyyy).</summary>
    [CampoSped(Ordem = 8, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Numero do documento de arrecadacao estadual, se houver.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0)]
    public string? NumDa { get; set; }
}
