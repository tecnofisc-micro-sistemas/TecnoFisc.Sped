using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C175 — Complemento de Item: Operações com Veículos Novos (cód. 01 e 55).
/// Nível hierárquico 4, ocorrência 1:N (por registro C170).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 84.
/// </summary>
[RegistroSped(Codigo = "C175", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC175 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C175";

    /// <summary>Indicador do tipo de operação com veículo.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacaoVeiculo? IndVeicOper { get; set; }

    /// <summary>CNPJ da concessionária (14 dígitos, sem máscara). Obrigatório quando IND_VEIC_OPER = 1.</summary>
    [CampoSped(Ordem = 3, Tamanho = 14)]
    public Cnpj? Cnpj { get; set; }

    /// <summary>Sigla da UF da concessionária.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2)]
    public string? Uf { get; set; }

    /// <summary>Chassi do veículo (17 caracteres).</summary>
    [CampoSped(Ordem = 5, Tamanho = 17, Obrigatorio = true)]
    public string? ChassiVeic { get; set; }
}
