using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E530 — Ajustes da Apuração do IPI.
/// Nível hierárquico 4, ocorrência 1:N por período. Conforme Guia Prático EFD-ICMS/IPI
/// V3.0.6, p. 233-235.
/// </summary>
[RegistroSped(Codigo = "E530", Nivel = 4, Bloco = "E")]
public sealed partial class RegistroE530 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E530";

    /// <summary>Indicador do tipo de ajuste: débito ou crédito.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoAjusteIpi IndAj { get; set; }

    /// <summary>Valor do ajuste lançado nos campos outros débitos ou outros créditos.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAj { get; set; }

    /// <summary>Código do ajuste da apuração, conforme Tabela 4.5.4.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true)]
    public string? CodAj { get; set; }

    /// <summary>Indicador da origem do documento vinculado ao ajuste.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOrigemDocumentoAjusteIpi IndDoc { get; set; }

    /// <summary>Número do documento, processo ou declaração ao qual o ajuste está vinculado, se houver.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? NumDoc { get; set; }

    /// <summary>Descrição resumida do ajuste, incluindo o período a que se refere, quando aplicável.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Obrigatorio = true)]
    public string? DescrAj { get; set; }
}
