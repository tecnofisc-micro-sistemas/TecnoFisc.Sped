using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoB;

/// <summary>
/// Registro B510 — Uniprofissional: Empregados e Sócios.
/// Nível hierárquico 3, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 57.
/// </summary>
[RegistroSped(Codigo = "B510", Nivel = 3, Bloco = "B")]
public sealed partial class RegistroB510 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "B510";

    /// <summary>Indicador de habilitação do profissional.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorHabilitacaoProfissional IndProf { get; set; }

    /// <summary>Indicador de escolaridade do profissional.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEscolaridade IndEsc { get; set; }

    /// <summary>Indicador de participação societária do profissional.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public IndicadorParticipacaoSocietaria IndSoc { get; set; }

    /// <summary>Número de inscrição do profissional no CPF (11 dígitos). Validação: DV conferido.</summary>
    [CampoSped(Ordem = 5, Tamanho = 11, Obrigatorio = true)]
    public Cpf Cpf { get; set; }

    /// <summary>Nome do profissional.</summary>
    [CampoSped(Ordem = 6, Tamanho = 100, Obrigatorio = true)]
    public string? Nome { get; set; }
}
