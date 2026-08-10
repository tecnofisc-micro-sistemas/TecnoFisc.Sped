using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoJ;

/// <summary>Registro J051 - mapeamento para o plano de contas referencial.</summary>
[RegistroSped(Codigo = "J051", Nivel = 3, Bloco = "J")]
public sealed partial class RegistroJ051 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J051";

    /// <summary>Código do centro de custos.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Nome = "COD_CCUS")]
    public string? CodCcus { get; set; }

    /// <summary>Código da conta no plano de contas referencial.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true, Nome = "COD_CTA_REF")]
    public string? CodCtaRef { get; set; }
}
