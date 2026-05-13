using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoH;

/// <summary>
/// Registro H020 — Informação Complementar do Inventário.
/// Nível hierárquico 4, ocorrência 1:1 por registro H010. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 248.
/// </summary>
[RegistroSped(Codigo = "H020", Nivel = 4, Bloco = "H")]
public sealed partial class RegistroH020 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "H020";

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme Tabela 4.3.1.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public string? CstIcms { get; set; }

    /// <summary>Base de cálculo do ICMS aplicável ao item.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? BcIcms { get; set; }

    /// <summary>Valor do ICMS a ser debitado ou creditado.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlIcms { get; set; }
}
