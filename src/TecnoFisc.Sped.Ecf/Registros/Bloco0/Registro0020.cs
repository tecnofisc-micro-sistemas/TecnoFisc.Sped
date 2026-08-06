using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Ecf.Versionamento;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco0;

/// <summary>Registro 0020 — parâmetros complementares.</summary>
[RegistroSped(Codigo = "0020", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0020 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0020";

    /// <summary>Indicador da alíquota de CSLL.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public string? IndAliqCsll { get; set; }

    /// <summary>Quantidade de sociedades em conta de participação.</summary>
    [CampoSped(Ordem = 3, Tamanho = 3, Obrigatorio = true)]
    public int IndQteScp { get; set; }

    /// <summary>Indicador de administração de fundos ou clubes de investimento.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndAdmFunClu { get; set; }

    /// <summary>Indicador de participações em consórcios de empresas.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndPartCons { get; set; }

    /// <summary>Indicador de operações com o exterior.</summary>
    [CampoSped(Ordem = 6, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndOpExt { get; set; }

    /// <summary>Indicador de operações com pessoa vinculada ou interposta.</summary>
    [CampoSped(Ordem = 7, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndOpVinc { get; set; }

    /// <summary>Indicador de enquadramento da pessoa jurídica.</summary>
    [CampoSped(Ordem = 8, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndPjEnquad { get; set; }

    /// <summary>Indicador de participações no exterior.</summary>
    [CampoSped(Ordem = 9, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndPartExt { get; set; }

    /// <summary>Indicador de atividade rural.</summary>
    [CampoSped(Ordem = 10, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndAtivRural { get; set; }

    /// <summary>Indicador de lucros, rendimentos ou ganhos de capital do exterior.</summary>
    [CampoSped(Ordem = 11, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndLucExp { get; set; }

    /// <summary>Indicador de isenção ou redução de imposto.</summary>
    [CampoSped(Ordem = 12, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndRedIsen { get; set; }

    /// <summary>Indicador de instituição financeira.</summary>
    [CampoSped(Ordem = 13, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndFin { get; set; }

    /// <summary>Indicador de participações avaliadas pelo método da equivalência patrimonial.</summary>
    [CampoSped(Ordem = 14, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndPartColig { get; set; }

    /// <summary>Indicador de recebimentos do exterior ou de não residentes.</summary>
    [CampoSped(Ordem = 15, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndRecExt { get; set; }

    /// <summary>Indicador de ativos no exterior.</summary>
    [CampoSped(Ordem = 16, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndAtivExt { get; set; }

    /// <summary>Indicador de pagamentos ao exterior ou a não residentes.</summary>
    [CampoSped(Ordem = 17, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndPgtoExt { get; set; }

    /// <summary>Indicador de comércio eletrônico e tecnologia da informação.</summary>
    [CampoSped(Ordem = 18, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndEComTi { get; set; }

    /// <summary>Indicador de recebimento de royalties.</summary>
    [CampoSped(Ordem = 19, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndRoyRec { get; set; }

    /// <summary>Indicador de pagamento de royalties.</summary>
    [CampoSped(Ordem = 20, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndRoyPag { get; set; }

    /// <summary>Indicador de rendimentos relativos a serviços.</summary>
    [CampoSped(Ordem = 21, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndRendServ { get; set; }

    /// <summary>Indicador de pagamentos ou remessas por serviços.</summary>
    [CampoSped(Ordem = 22, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndPgtoRem { get; set; }

    /// <summary>Indicador de inovação tecnológica e desenvolvimento tecnológico.</summary>
    [CampoSped(Ordem = 23, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndInovTec { get; set; }

    /// <summary>Indicador de capacitação de informática e inclusão digital.</summary>
    [CampoSped(Ordem = 24, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndCapInf { get; set; }

    /// <summary>Indicador de pessoa jurídica habilitada no Repes, Recap, Padis ou Reidi.</summary>
    [CampoSped(Ordem = 25, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndPjHab { get; set; }

    /// <summary>Indicador de polo industrial de Manaus e Amazônia Ocidental.</summary>
    [CampoSped(Ordem = 26, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndPoloAm { get; set; }

    /// <summary>Indicador de Zonas de Processamento de Exportação.</summary>
    [CampoSped(Ordem = 27, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndZonExp { get; set; }

    /// <summary>Indicador de áreas de livre comércio.</summary>
    [CampoSped(Ordem = 28, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndAreaCom { get; set; }

    /// <summary>Indicador de declaração País-a-País.</summary>
    [CampoSped(Ordem = 29, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndPaisAPais { get; set; }

    /// <summary>Indicador de adesão ao Derex.</summary>
    [CampoSped(Ordem = 30, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndDerex { get; set; }

    /// <summary>
    /// Valor posicional do campo 31: <c>IND_PR_TRANSF</c> nos leiautes 10 e 11 e
    /// <c>POSSUI_CEBRAS</c> no leiaute 12.
    /// </summary>
    [CampoSped(
        Ordem = 31,
        Nome = "POSSUI_CEBRAS",
        Tamanho = 1,
        Obrigatorio = true,
        DesdeVersao = (int)LayoutEcf.V010)]
    public IndicadorSimNao IndicadorPosicao31 { get; set; }

    /// <summary>
    /// Semântica do campo 31 nos leiautes 10 e 11: indicador de opção pelas novas regras de
    /// preços de transferência.
    /// </summary>
    public IndicadorSimNao IndPrTransf
    {
        get => IndicadorPosicao31;
        set => IndicadorPosicao31 = value;
    }

    /// <summary>Semântica do campo 31 no leiaute 12: indicador de posse de certificado Cebas.</summary>
    public IndicadorSimNao PossuiCebras
    {
        get => IndicadorPosicao31;
        set => IndicadorPosicao31 = value;
    }

    /// <summary>Número do Cebas, condicionado ao indicador de posse.</summary>
    [CampoSped(Ordem = 32, Tamanho = 255, DesdeVersao = (int)LayoutEcf.V012)]
    public string? Cebas { get; set; }
}
