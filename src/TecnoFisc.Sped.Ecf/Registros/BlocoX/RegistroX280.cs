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

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public BeneficioFiscalIncentivado IndAtiv { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public OrgaoConcedenteIncentivo IndConcedente { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true)]
    public ProjetoIncentivado IndProj { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 30, Obrigatorio = true)]
    public string? AtoConc { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly VigIni { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly VigFim { get; set; }

    /// <summary>CNPJ do estabelecimento beneficiado.</summary>
    [CampoSped(Ordem = 8, Tamanho = 14, Obrigatorio = true)]
    public Cnpj CnpjIncentivo { get; set; }

    /// <summary>NCM dinâmico preservado como código lexical.</summary>
    [CampoSped(Ordem = 9, Tamanho = 8)]
    public string? NcmIncentivo { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal RecLiqIncentivo { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 19, Decimais = 2)]
    public decimal? VlIncentivo { get; set; }
}
