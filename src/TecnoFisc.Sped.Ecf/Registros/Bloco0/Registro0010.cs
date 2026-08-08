using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco0;

/// <summary>Registro 0010 — parâmetros de tributação.</summary>
[RegistroSped(Codigo = "0010", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0010 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0010";

    /// <summary>Hash da ECF do período imediatamente anterior.</summary>
    [CampoSped(Ordem = 2, Tamanho = 40, Nome = "HASH_ECF_ANTERIOR")]
    public string? HashEcfAnterior { get; set; }

    /// <summary>Indicador de opção pelo Refis.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true, Nome = "OPT_REFIS")]
    public IndicadorSimNao OptRefis { get; set; }

    /// <summary>Forma de tributação do lucro.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true, Nome = "FORMA_TRIB")]
    public string? FormaTrib { get; set; }

    /// <summary>Forma de apuração.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Nome = "FORMA_APUR")]
    public string? FormaApur { get; set; }

    /// <summary>Qualificação da pessoa jurídica.</summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Nome = "COD_QUALIF_PJ")]
    public string? CodQualifPj { get; set; }

    /// <summary>Forma de tributação por período.</summary>
    [CampoSped(Ordem = 7, Tamanho = 4, Nome = "FORMA_TRIB_PER")]
    public string? FormaTribPer { get; set; }

    /// <summary>Meses de balanço ou balancete de redução.</summary>
    [CampoSped(Ordem = 8, Tamanho = 12, Nome = "MES_BAL_RED")]
    public string? MesBalRed { get; set; }

    /// <summary>Tipo de escrituração.</summary>
    [CampoSped(Ordem = 9, Tamanho = 1, Nome = "TIP_ESC_PRE")]
    public string? TipEscPre { get; set; }

    /// <summary>Tipo de entidade.</summary>
    [CampoSped(Ordem = 10, Tamanho = 2, Nome = "TIP_ENT")]
    public string? TipEnt { get; set; }

    /// <summary>Forma de apuração das estimativas mensais.</summary>
    [CampoSped(Ordem = 11, Tamanho = 1, Nome = "FORMA_APUR_I")]
    public string? FormaApurI { get; set; }

    /// <summary>Forma de apuração da CSLL.</summary>
    [CampoSped(Ordem = 12, Tamanho = 1, Nome = "APUR_CSLL")]
    public string? ApurCsll { get; set; }

    /// <summary>Indicador de reconhecimento de receitas.</summary>
    [CampoSped(Ordem = 13, Tamanho = 1, Nome = "IND_REC_RECEITA")]
    public string? IndRecReceita { get; set; }
}
