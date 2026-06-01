using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1923 - Informacoes adicionais dos ajustes da sub-apuracao do ICMS -
/// identificacao dos documentos fiscais.
/// Nivel hierarquico 6, ocorrencia 1:N. Conforme Guia Pratico EFD-ICMS/IPI
/// V3.0.6, p. 294.
/// </summary>
[RegistroSped(Codigo = "1923", Nivel = 6, Bloco = "1")]
public sealed partial class Registro1923 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1923";

    /// <summary>Codigo do participante: emitente/remetente nas entradas ou adquirente nas saidas.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodPart { get; set; }

    /// <summary>Codigo do modelo do documento fiscal, conforme a Tabela 4.1.1.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Serie do documento fiscal.</summary>
    [CampoSped(Ordem = 4, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Subserie do documento fiscal.</summary>
    [CampoSped(Ordem = 5, Tamanho = 3)]
    public int? Sub { get; set; }

    /// <summary>Numero do documento fiscal.</summary>
    [CampoSped(Ordem = 6, Tamanho = 9, Obrigatorio = true)]
    public int? NumDoc { get; set; }

    /// <summary>Data da emissao do documento fiscal no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Codigo do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 8, Tamanho = 60)]
    public string? CodItem { get; set; }

    /// <summary>Valor do ajuste para a operacao/item.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjItem { get; set; }

    /// <summary>Chave do Documento Eletronico.</summary>
    [CampoSped(Ordem = 10, Tamanho = 44)]
    public ChaveAcesso? ChvDoce { get; set; }
}
