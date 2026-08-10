using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoM;

/// <summary>Registro M355 - conta da Parte B relacionada ao e-Lacs.</summary>
[RegistroSped(Codigo = "M355", Nivel = 4, Bloco = "M")]
public sealed partial class RegistroM355 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M355";

    /// <summary>Código da Conta na Parte B: Código unívoco atribuído pelo contribuinte à conta no e-Lacs no registro M010.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "COD_CTA_B")]
    public string? CodCtaB { get; set; }

    /// <summary>Valor Total dos Lançamentos: Valor total dos lançamentos adicionados ou excluídos da conta. Observação:Valor deve ser menor ou igual ao saldo disponível do mesmo período de apuração da conta na parte B do registro M410.</summary>
    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_CTA")]
    public decimal VlCta { get; set; }

    /// <summary>Indicador do Valor Total dos Lançamentos: D – Para prejuízos ou valores que reduzam o lucro real em períodos subsequentes. C – Para valores que aumentam o lucro real em períodos subsequentes.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true, Nome = "IND_VL_CTA")]
    public IndicadorDebitoCredito IndVlCta { get; set; }
}
