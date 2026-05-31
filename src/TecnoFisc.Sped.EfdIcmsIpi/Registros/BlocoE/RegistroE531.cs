using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E531 — Informações Adicionais dos Ajustes da Apuração do IPI — Identificação
/// dos Documentos Fiscais (01 e 55). Nível hierárquico 5, ocorrência 1:N. Conforme Guia
/// Prático EFD-ICMS/IPI V3.0.6, p. 235-236.
/// </summary>
[RegistroSped(Codigo = "E531", Nivel = 5, Bloco = "E")]
public sealed partial class RegistroE531 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E531";

    /// <summary>Código do participante (campo 02 do Registro 0150): emitente/remetente nas entradas ou adquirente nas saídas.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Código do modelo do documento fiscal, conforme a Tabela 4.1.1. Valores válidos no registro: 01 e 55.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public ModeloDocumento? CodMod { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 4, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal.</summary>
    [CampoSped(Ordem = 5, Tamanho = 3)]
    public int? Sub { get; set; }

    /// <summary>Número do documento fiscal.</summary>
    [CampoSped(Ordem = 6, Tamanho = 9, Obrigatorio = true)]
    public int? NumDoc { get; set; }

    /// <summary>Data da emissão do documento fiscal no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 8, Tamanho = 60)]
    public string? CodItem { get; set; }

    /// <summary>Valor do ajuste para a operação/item.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjItem { get; set; }

    /// <summary>Chave da Nota Fiscal Eletrônica (modelo 55).</summary>
    [CampoSped(Ordem = 10, Tamanho = 44)]
    public ChaveAcesso? ChvNfe { get; set; }
}
