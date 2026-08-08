using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco9;

/// <summary>Registro 9100 - avisos da escrituração.</summary>
[RegistroSped(Codigo = "9100", Nivel = 2, Bloco = "9")]
public sealed partial class Registro9100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9100";

    /// <summary>Identificação numérica da regra, preservada lexicalmente.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "NOM_REGRA")]
    public string? NomRegra { get; set; }

    /// <summary>Mensagem associada ao aviso.</summary>
    [CampoSped(Ordem = 3, Nome = "MSG_REGRA")]
    public string? MsgRegra { get; set; }

    /// <summary>Registro onde ocorreu o aviso.</summary>
    [CampoSped(Ordem = 4, Tamanho = 4, Obrigatorio = true, Nome = "REGISTRO")]
    public string? Registro { get; set; }

    /// <summary>Nome do campo onde ocorreu o aviso.</summary>
    [CampoSped(Ordem = 5, Nome = "CAMPO")]
    public string? Campo { get; set; }

    /// <summary>Conteúdo numérico preenchido pelo usuário.</summary>
    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Nome = "CONTEÚDO")]
    public decimal? Conteudo { get; set; }

    /// <summary>Valor original ou calculado esperado.</summary>
    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Nome = "VALOR_ESPERADO")]
    public decimal? ValorEsperado { get; set; }

    /// <summary>Período de apuração fiscal.</summary>
    [CampoSped(Ordem = 8, Tamanho = 3, Nome = "PER_APUR")]
    public string? PerApur { get; set; }

    /// <summary>Código da conta.</summary>
    [CampoSped(Ordem = 9, Nome = "COD_CTA")]
    public string? CodCta { get; set; }

    /// <summary>Código do centro de custos.</summary>
    [CampoSped(Ordem = 10, Nome = "COD_CCUS")]
    public string? CodCcus { get; set; }

    /// <summary>Código da conta referencial.</summary>
    [CampoSped(Ordem = 11, Nome = "COD_CTA_REF")]
    public string? CodCtaRef { get; set; }

    /// <summary>Código da linha na tabela dinâmica.</summary>
    [CampoSped(Ordem = 12, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Número de ordem do registro X300.</summary>
    [CampoSped(Ordem = 13, Nome = "NUM_ORDEM")]
    public string? NumOrdem { get; set; }

    /// <summary>CNPJ do estabelecimento do registro Y540.</summary>
    [CampoSped(Ordem = 14, Tamanho = 14, Nome = "CNPJ_ESTAB")]
    public Cnpj? CnpjEstab { get; set; }

    /// <summary>CNAE do registro Y540, preservado com zeros à esquerda.</summary>
    [CampoSped(Ordem = 15, Tamanho = 7, Nome = "CNAE")]
    public string? Cnae { get; set; }

    /// <summary>Código da conta da Parte B.</summary>
    [CampoSped(Ordem = 16, Nome = "COD_CTA_B")]
    public string? CodCtaB { get; set; }

    /// <summary>Tributo relacionado à conta da Parte B.</summary>
    [CampoSped(Ordem = 17, Nome = "COD_TRIBUTO")]
    public string? CodTributo { get; set; }
}
