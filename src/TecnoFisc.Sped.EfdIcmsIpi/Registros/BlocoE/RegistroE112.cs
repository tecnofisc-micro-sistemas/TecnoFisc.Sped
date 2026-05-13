using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E112 — Informações Adicionais dos Ajustes da Apuração do ICMS.
/// Nível hierárquico 5, ocorrência 1:N. Conforme Guia Prático EFD-ICMS/IPI
/// V3.0.6, p. 210.
/// </summary>
[RegistroSped(Codigo = "E112", Nivel = 5, Bloco = "E")]
public sealed partial class RegistroE112 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E112";

    /// <summary>Número do documento de arrecadação estadual, se houver.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0)]
    public string? NumDa { get; set; }

    /// <summary>Número do processo ao qual o ajuste está vinculado, se houver.</summary>
    [CampoSped(Ordem = 3, Tamanho = 15)]
    public string? NumProc { get; set; }

    /// <summary>Indicador da origem do processo: 0-SEFAZ; 1-Justiça Federal; 2-Justiça Estadual; 9-Outros.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1)]
    public IndicadorOrigemProcesso? IndProc { get; set; }

    /// <summary>Descrição resumida do processo que embasou o lançamento.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? Proc { get; set; }

    /// <summary>Descrição complementar.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? TxtCompl { get; set; }
}
