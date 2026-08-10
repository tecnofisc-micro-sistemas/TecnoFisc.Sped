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
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true, Nome = "IND_ALIQ_CSLL")]
    public string? IndAliqCsll { get; set; }

    /// <summary>Quantidade de sociedades em conta de participação.</summary>
    [CampoSped(Ordem = 3, Tamanho = 3, Obrigatorio = true, Nome = "IND_QTE_SCP")]
    public int IndQteScp { get; set; }

    /// <summary>Indicador de administração de fundos ou clubes de investimento.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true, Nome = "IND_ADM_FUN_CLU")]
    public IndicadorSimNao IndAdmFunClu { get; set; }

    /// <summary>Indicador de participações em consórcios de empresas.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true, Nome = "IND_PART_CONS")]
    public IndicadorSimNao IndPartCons { get; set; }

    /// <summary>Indicador de operações com o exterior.</summary>
    [CampoSped(Ordem = 6, Tamanho = 1, Obrigatorio = true, Nome = "IND_OP_EXT")]
    public IndicadorSimNao IndOpExt { get; set; }

    /// <summary>Indicador de operações com pessoa vinculada ou interposta.</summary>
    [CampoSped(Ordem = 7, Tamanho = 1, Obrigatorio = true, Nome = "IND_OP_VINC")]
    public IndicadorSimNao IndOpVinc { get; set; }

    /// <summary>Indicador de enquadramento da pessoa jurídica.</summary>
    [CampoSped(Ordem = 8, Tamanho = 1, Obrigatorio = true, Nome = "IND_PJ_ENQUAD")]
    public IndicadorSimNao IndPjEnquad { get; set; }

    /// <summary>Indicador de participações no exterior.</summary>
    [CampoSped(Ordem = 9, Tamanho = 1, Obrigatorio = true, Nome = "IND_PART_EXT")]
    public IndicadorSimNao IndPartExt { get; set; }

    /// <summary>Indicador de atividade rural.</summary>
    [CampoSped(Ordem = 10, Tamanho = 1, Obrigatorio = true, Nome = "IND_ATIV_RURAL")]
    public IndicadorSimNao IndAtivRural { get; set; }

    /// <summary>Indicador de lucros, rendimentos ou ganhos de capital do exterior.</summary>
    [CampoSped(Ordem = 11, Tamanho = 1, Obrigatorio = true, Nome = "IND_LUC_EXP")]
    public IndicadorSimNao IndLucExp { get; set; }

    /// <summary>Indicador de isenção ou redução de imposto.</summary>
    [CampoSped(Ordem = 12, Tamanho = 1, Obrigatorio = true, Nome = "IND_RED_ISEN")]
    public IndicadorSimNao IndRedIsen { get; set; }

    /// <summary>Indicador de instituição financeira.</summary>
    [CampoSped(Ordem = 13, Tamanho = 1, Obrigatorio = true, Nome = "IND_FIN")]
    public IndicadorSimNao IndFin { get; set; }

    /// <summary>Indicador de participações avaliadas pelo método da equivalência patrimonial.</summary>
    [CampoSped(Ordem = 14, Tamanho = 1, Obrigatorio = true, Nome = "IND_PART_COLIG")]
    public IndicadorSimNao IndPartColig { get; set; }

    /// <summary>Indicador de recebimentos do exterior ou de não residentes.</summary>
    [CampoSped(Ordem = 15, Tamanho = 1, Obrigatorio = true, Nome = "IND_REC_EXT")]
    public IndicadorSimNao IndRecExt { get; set; }

    /// <summary>Indicador de ativos no exterior.</summary>
    [CampoSped(Ordem = 16, Tamanho = 1, Obrigatorio = true, Nome = "IND_ATIV_EXT")]
    public IndicadorSimNao IndAtivExt { get; set; }

    /// <summary>Indicador de pagamentos ao exterior ou a não residentes.</summary>
    [CampoSped(Ordem = 17, Tamanho = 1, Obrigatorio = true, Nome = "IND_PGTO_EXT")]
    public IndicadorSimNao IndPgtoExt { get; set; }

    /// <summary>
    /// Indicador de comércio eletrônico e tecnologia da informação.
    /// Nome normativo do manual (pág. 81) é <c>IND_E-COM_TI</c>, com hífen deliberado
    /// ("E-COM" = e-commerce). O alias abaixo normaliza o hífen para underscore porque
    /// não é um separador válido em <c>Nome</c> (CatalogoBuilder.IsFieldNameValid); não é
    /// erro de extração do PDF.
    /// </summary>
    [CampoSped(Ordem = 18, Tamanho = 1, Obrigatorio = true, Nome = "IND_E_COM_TI")]
    public IndicadorSimNao IndEComTi { get; set; }

    /// <summary>Indicador de recebimento de royalties.</summary>
    [CampoSped(Ordem = 19, Tamanho = 1, Obrigatorio = true, Nome = "IND_ROY_REC")]
    public IndicadorSimNao IndRoyRec { get; set; }

    /// <summary>Indicador de pagamento de royalties.</summary>
    [CampoSped(Ordem = 20, Tamanho = 1, Obrigatorio = true, Nome = "IND_ROY_PAG")]
    public IndicadorSimNao IndRoyPag { get; set; }

    /// <summary>Indicador de rendimentos relativos a serviços.</summary>
    [CampoSped(Ordem = 21, Tamanho = 1, Obrigatorio = true, Nome = "IND_REND_SERV")]
    public IndicadorSimNao IndRendServ { get; set; }

    /// <summary>Indicador de pagamentos ou remessas por serviços.</summary>
    [CampoSped(Ordem = 22, Tamanho = 1, Obrigatorio = true, Nome = "IND_PGTO_REM")]
    public IndicadorSimNao IndPgtoRem { get; set; }

    /// <summary>Indicador de inovação tecnológica e desenvolvimento tecnológico.</summary>
    [CampoSped(Ordem = 23, Tamanho = 1, Obrigatorio = true, Nome = "IND_INOV_TEC")]
    public IndicadorSimNao IndInovTec { get; set; }

    /// <summary>Indicador de capacitação de informática e inclusão digital.</summary>
    [CampoSped(Ordem = 24, Tamanho = 1, Obrigatorio = true, Nome = "IND_CAP_INF")]
    public IndicadorSimNao IndCapInf { get; set; }

    /// <summary>Indicador de pessoa jurídica habilitada no Repes, Recap, Padis ou Reidi.</summary>
    [CampoSped(Ordem = 25, Tamanho = 1, Obrigatorio = true, Nome = "IND_PJ_HAB")]
    public IndicadorSimNao IndPjHab { get; set; }

    /// <summary>Indicador de polo industrial de Manaus e Amazônia Ocidental.</summary>
    [CampoSped(Ordem = 26, Tamanho = 1, Obrigatorio = true, Nome = "IND_POLO_AM")]
    public IndicadorSimNao IndPoloAm { get; set; }

    /// <summary>Indicador de Zonas de Processamento de Exportação.</summary>
    [CampoSped(Ordem = 27, Tamanho = 1, Obrigatorio = true, Nome = "IND_ZON_EXP")]
    public IndicadorSimNao IndZonExp { get; set; }

    /// <summary>Indicador de áreas de livre comércio.</summary>
    [CampoSped(Ordem = 28, Tamanho = 1, Obrigatorio = true, Nome = "IND_AREA_COM")]
    public IndicadorSimNao IndAreaCom { get; set; }

    /// <summary>Indicador de declaração País-a-País.</summary>
    [CampoSped(Ordem = 29, Tamanho = 1, Obrigatorio = true, Nome = "IND_PAIS_A_PAIS")]
    public IndicadorSimNao IndPaisAPais { get; set; }

    /// <summary>Indicador de adesão ao Derex.</summary>
    [CampoSped(Ordem = 30, Tamanho = 1, Obrigatorio = true, Nome = "IND_DEREX")]
    public IndicadorSimNao IndDerex { get; set; }

    /// <summary>
    /// Valor posicional do campo 31, cuja semântica depende do <c>COD_VER</c> do arquivo:
    /// <c>IND_PR_TRANSF</c> (opção pelas novas regras de preços de transferência) nos leiautes
    /// 10 e 11, e <c>POSSUI_CEBRAS</c> (posse de certificado Cebas) no leiaute 12. Não há como o
    /// registro saber qual das duas é a sua sem a versão do arquivo — consulte o <c>COD_VER</c>
    /// do <c>0000</c> antes de interpretar este valor. O rótulo do atributo (<c>Nome</c>)
    /// permanece <c>POSSUI_CEBRAS</c> porque o manifesto do leiaute 12 — a verdade sobre o
    /// leiaute vigente, contra a qual <c>ManifestoCatalogoTests</c> compara o catálogo campo a
    /// campo — descreve o campo 31 assim; um rótulo composto (<c>"IND_PR_TRANSF/POSSUI_CEBRAS"</c>)
    /// além de divergir do manifesto também não passaria em <c>CatalogoBuilder.IsFieldNameValid</c>
    /// (a barra não é caractere válido em nome de campo).
    /// </summary>
    [CampoSped(Ordem = 31, Tamanho = 1, Obrigatorio = true, DesdeVersao = (int)LayoutEcf.V010, Nome = "POSSUI_CEBRAS")]
    public IndicadorSimNao IndicadorPosicao31 { get; set; }

    /// <summary>Número do Cebas, condicionado ao indicador de posse.</summary>
    [CampoSped(Ordem = 32, Tamanho = 255, DesdeVersao = (int)LayoutEcf.V012, Nome = "CEBAS")]
    public string? Cebas { get; set; }

    /// <summary>
    /// Semântica do campo 31 nos leiautes 10 e 11: opção pelas novas regras de preços de
    /// transferência (<c>IND_PR_TRANSF</c>). <c>null</c> em qualquer outro leiaute, onde a posição
    /// significa outra coisa — ver <see cref="IndicadorPosicao31"/>. Propriedade calculada: não é
    /// campo do catálogo, e depende de <see cref="RegistroSped.VersaoDoArquivo"/>, que só é
    /// preenchida em registro vindo de leitura de arquivo.
    /// </summary>
    public IndicadorSimNao? IndPrTransf
        => VersaoDoArquivo is 10 or 11 ? IndicadorPosicao31 : null;

    /// <summary>
    /// Semântica do campo 31 a partir do leiaute 12: posse de certificado Cebas
    /// (<c>POSSUI_CEBRAS</c>). <c>null</c> nos leiautes anteriores — ver
    /// <see cref="IndicadorPosicao31"/>. Responde também em leiaute posterior ao 12, ainda não
    /// modelado: enquanto a Receita não reaproveitar a posição de novo, a leitura vigente é essa.
    /// </summary>
    public IndicadorSimNao? PossuiCebras
        => VersaoDoArquivo >= 12 ? IndicadorPosicao31 : null;
}
