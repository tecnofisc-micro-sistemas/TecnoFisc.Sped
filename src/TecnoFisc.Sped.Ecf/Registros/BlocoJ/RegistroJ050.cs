using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoJ;

/// <summary>Registro J050 - plano de contas do contribuinte.</summary>
[RegistroSped(Codigo = "J050", Nivel = 2, Bloco = "J")]
public sealed partial class RegistroJ050 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J050";

    /// <summary>Data de inclusão ou alteração da conta.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtAlt { get; set; }

    /// <summary>Natureza da conta analítica ou sintética.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public CodigoNaturezaContaContabil CodNat { get; set; }

    /// <summary>Indicador do tipo de conta.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public IndicadorTipoConta IndCta { get; set; }

    /// <summary>Nível da conta analítica ou sintética.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public int Nível { get; set; }

    /// <summary>Código da conta analítica ou sintética.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Obrigatorio = true)]
    public string? CodCta { get; set; }

    /// <summary>Código da conta sintética de nível imediatamente superior.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0)]
    public string? CodCtaSup { get; set; }

    /// <summary>Nome da conta.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Obrigatorio = true)]
    public string? Cta { get; set; }
}
