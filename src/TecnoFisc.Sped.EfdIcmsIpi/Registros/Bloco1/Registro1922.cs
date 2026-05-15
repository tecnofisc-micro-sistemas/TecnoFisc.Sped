using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1922 - Informacoes adicionais dos ajustes da sub-apuracao do ICMS.
/// Nivel hierarquico 6, ocorrencia 1:N. Conforme Guia Pratico EFD-ICMS/IPI
/// V3.0.6, p. 293.
/// </summary>
[RegistroSped(Codigo = "1922", Nivel = 6, Bloco = "1")]
public sealed partial class Registro1922 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1922";

    /// <summary>Numero do documento de arrecadacao estadual, se houver.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0)]
    public string? NumDa { get; set; }

    /// <summary>Numero do processo ao qual o ajuste esta vinculado, se houver.</summary>
    [CampoSped(Ordem = 3, Tamanho = 15)]
    public string? NumProc { get; set; }

    /// <summary>Indicador da origem do processo: 0-SEFAZ; 1-Justica Federal; 2-Justica Estadual; 9-Outros.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1)]
    public IndicadorOrigemProcesso? IndProc { get; set; }

    /// <summary>Descricao resumida do processo que embasou o lancamento.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? Proc { get; set; }

    /// <summary>Descricao complementar.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? TxtCompl { get; set; }
}
