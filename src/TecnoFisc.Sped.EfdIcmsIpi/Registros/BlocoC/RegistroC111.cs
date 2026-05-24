using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C111 — Processo Referenciado.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 67.
/// </summary>
[RegistroSped(Codigo = "C111", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC111 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C111";

    /// <summary>
    /// Identificação do processo ou ato concessório. Tamanho expandido para 60 em V017
    /// (Guia Prático 3.1.0 itens 8-10).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? NumProc { get; set; }

    /// <summary>Indicador da origem do processo: 0-SEFAZ; 1-Justiça Federal; 2-Justiça Estadual; 3-SECEX/SRF; 9-Outros.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOrigemProcesso IndProc { get; set; }
}
