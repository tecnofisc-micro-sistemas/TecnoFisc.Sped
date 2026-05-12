using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C174 — Complemento de Item: Operações com Armas de Fogo (cód. 01).
/// Nível hierárquico 4, ocorrência 1:N (por registro C170).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 84.
/// </summary>
[RegistroSped(Codigo = "C174", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC174 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C174";

    /// <summary>Indicador do tipo da arma de fogo.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1)]
    public IndicadorTipoArmaFogo? IndArm { get; set; }

    /// <summary>Numeração de série de fabricação da arma.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? NumArm { get; set; }

    /// <summary>Descrição da arma (cano, calibre, marca, capacidade de cartuchos etc.).</summary>
    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? DescrCompl { get; set; }
}
