using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoM;

/// <summary>Registro M310 - contas contábeis relacionadas à Parte A do e-Lalur.</summary>
[RegistroSped(Codigo = "M310", Nivel = 4, Bloco = "M")]
public sealed partial class RegistroM310 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M310";

    /// <summary>Código da Conta Contábil (Plano de Contas da Pessoa Jurídica): Código da conta ou subconta contábil onde está registrado o valor a ser adicionado ou excluído, quando possível sua identificação (deve existir no J050).</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public string? CodCta { get; set; }

    /// <summary>Código do Centro de Custos (deve existir no J100).</summary>
    [CampoSped(Ordem = 3)]
    public string? CodCcus { get; set; }

    /// <summary>Valor da Conta Utilizado no Lançamento da Parte A.</summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlCta { get; set; }

    /// <summary>Indicador do Valor do Lançamento: D – Devedor. C – Credor.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndVlCta { get; set; }
}
