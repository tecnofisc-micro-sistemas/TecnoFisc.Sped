using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoC;

/// <summary>
/// Registro C650 — Demonstração do Resultado do Exercício Recuperada. Nível hierárquico 3, ocorrência 1:N.
/// Traz a DRE recuperada da ECD anterior, detalhando cada linha pelo código de aglutinação.
/// Campo(s) chave: [COD_AGL].
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 100.
/// </summary>
[RegistroSped(Codigo = "C650", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC650 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C650";

    /// <summary>Código de aglutinação da linha.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodAgl { get; set; }

    /// <summary>Nível do código de aglutinação da linha.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public int NivelAgl { get; set; }

    /// <summary>Descrição do código de aglutinação da linha.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true)]
    public string? DescrCodAgl { get; set; }

    /// <summary>Valor do saldo final da linha.</summary>
    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlCtaFin { get; set; }

    /// <summary>Indicador da situação do saldo final da linha: D = Devedor, C = Credor.</summary>
    [CampoSped(Ordem = 6, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndDcCtaFin { get; set; }
}
