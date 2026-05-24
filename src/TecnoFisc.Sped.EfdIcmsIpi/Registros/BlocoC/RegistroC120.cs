using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C120 — Complemento — Operações de Importação (cód. 01 e 55).
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 72.
/// </summary>
[RegistroSped(Codigo = "C120", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC120 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C120";

    /// <summary>Código do documento de importação (0 = DI; 1 = DSI).</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public CodigoDocumentoImportacao? CodDocImp { get; set; }

    /// <summary>
    /// Número do documento de importação. Tamanho máximo do leiaute vigente: 15 caracteres
    /// (Guide 3.0.7 item 4, V016 — aumento de 12 para 15). Pacote read-only mantém o tamanho
    /// mais recente — arquivos V015 com até 12 caracteres continuam parseáveis.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 15, Obrigatorio = true)]
    public string? NumDocImp { get; set; }

    /// <summary>Valor pago de PIS na importação.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? PisImp { get; set; }

    /// <summary>Valor pago de COFINS na importação.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? CofinsImp { get; set; }

    /// <summary>Número do Ato Concessório do regime Drawback (até 20 caracteres).</summary>
    [CampoSped(Ordem = 6, Tamanho = 20)]
    public string? NumAcdraw { get; set; }
}
