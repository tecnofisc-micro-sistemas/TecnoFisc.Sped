using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E110 — Apuração do ICMS – Operações Próprias.
/// Nível hierárquico 3, ocorrência um por período. Conforme Guia Prático EFD-ICMS/IPI
/// V3.2.2, p. 223.
/// </summary>
/// <remarks>
/// <b>V018 (Guide 3.1.5 itens 12-17):</b> novas validações cruzadas dos totais com os
/// registros do bloco D que passam a integrar a apuração da NFCom e da escrituração
/// consolidada: campo 02 <c>VL_TOT_DEBITOS</c> (inclui D700/D730/D750/D760); campo 03
/// <c>VL_AJ_DEBITOS</c> (C800/C857/C860/C897/D700/D737); campo 06 <c>VL_TOT_CREDITOS</c>
/// (D700/D730); campo 07 <c>VL_AJ_CREDITOS</c> (C800/C857/C860/C897/D700/D737); campo 12
/// <c>VL_TOT_DED</c> (mesmos); campo 15 <c>DEB_ESP</c> (C857/C897/D737).
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </remarks>
[RegistroSped(Codigo = "E110", Nivel = 3, Bloco = "E")]
public sealed partial class RegistroE110 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E110";

    /// <summary>Valor total dos débitos por "Saídas e prestações com débito do imposto".</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotDebitos { get; set; }

    /// <summary>Valor total dos ajustes a débito decorrentes do documento fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjDebitos { get; set; }

    /// <summary>Valor total de "Ajustes a débito".</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotAjDebitos { get; set; }

    /// <summary>Valor total de Ajustes "Estornos de créditos".</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlEstornosCred { get; set; }

    /// <summary>Valor total dos créditos por "Entradas e aquisições com crédito do imposto".</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotCreditos { get; set; }

    /// <summary>Valor total dos ajustes a crédito decorrentes do documento fiscal.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjCreditos { get; set; }

    /// <summary>Valor total de "Ajustes a crédito".</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotAjCreditos { get; set; }

    /// <summary>Valor total de Ajustes "Estornos de Débitos".</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlEstornosDeb { get; set; }

    /// <summary>Valor total de "Saldo credor do período anterior".</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldCredorAnt { get; set; }

    /// <summary>Valor do saldo devedor apurado.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldApurado { get; set; }

    /// <summary>Valor total de "Deduções".</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotDed { get; set; }

    /// <summary>Valor total de "ICMS a recolher (11-12)".</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIcmsRecolher { get; set; }

    /// <summary>Valor total de "Saldo credor a transportar para o período seguinte".</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldCredorTransportar { get; set; }

    /// <summary>Valores recolhidos ou a recolher, extra-apuração.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal DebEsp { get; set; }
}
