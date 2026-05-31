using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E313 — Informações adicionais dos ajustes da apuração do Fundo de Combate à
/// Pobreza e do ICMS Diferencial de Alíquota — identificação dos documentos fiscais.
/// Nível hierárquico 5, ocorrência 1:N. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 229-230.
/// </summary>
[RegistroSped(Codigo = "E313", Nivel = 5, Bloco = "E")]
public sealed partial class RegistroE313 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E313";

    /// <summary>Código do participante (campo 02 do Registro 0150).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Código do modelo do documento fiscal, conforme a Tabela 4.1.1.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 4, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal.</summary>
    [CampoSped(Ordem = 5, Tamanho = 3)]
    public int? Sub { get; set; }

    /// <summary>Número do documento fiscal.</summary>
    [CampoSped(Ordem = 6, Tamanho = 9, Obrigatorio = true)]
    public int? NumDoc { get; set; }

    /// <summary>Chave do Documento Eletrônico.</summary>
    [CampoSped(Ordem = 7, Tamanho = 44)]
    public ChaveAcesso? ChvDoce { get; set; }

    /// <summary>Data da emissão do documento fiscal no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 8, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 9, Tamanho = 60)]
    public string? CodItem { get; set; }

    /// <summary>Valor do ajuste para a operação/item.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjItem { get; set; }
}
