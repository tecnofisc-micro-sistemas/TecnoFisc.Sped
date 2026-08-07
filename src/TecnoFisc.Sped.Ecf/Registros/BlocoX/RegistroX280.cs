using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X280 - atividades incentivadas da pessoa jurídica.</summary>
[RegistroSped(Codigo = "X280", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX280 : RegistroSped
{
    public override string Codigo => "X280";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true, Nome = "IND_ATIV")]
    public BeneficioFiscalIncentivado IndAtiv { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true, Nome = "IND_CONCEDENTE")]
    public OrgaoConcedenteIncentivo IndConcedente { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true, Nome = "IND_PROJ")]
    public ProjetoIncentivado IndProj { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 30, Obrigatorio = true, Nome = "ATO_CONC")]
    public string? AtoConc { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "VIG_INI")]
    public DateOnly VigIni { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "VIG_FIM")]
    public DateOnly VigFim { get; set; }

    /// <summary>CNPJ do estabelecimento beneficiado.</summary>
    [CampoSped(Ordem = 8, Tamanho = 14, Obrigatorio = true, Nome = "CNPJ_INCENTIVO")]
    public Cnpj CnpjIncentivo { get; set; }

    /// <summary>NCM dinâmico preservado como código lexical.</summary>
    [CampoSped(Ordem = 9, Tamanho = 8, Nome = "NCM_INCENTIVO")]
    public string? NcmIncentivo { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "REC_LIQ_INCENTIVO")]
    public decimal RecLiqIncentivo { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 19, Decimais = 2, Nome = "VL_INCENTIVO")]
    public decimal? VlIncentivo { get; set; }
}
