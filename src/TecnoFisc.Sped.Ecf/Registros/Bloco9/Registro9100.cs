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
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public string? NomRegra { get; set; }

    /// <summary>Mensagem associada ao aviso.</summary>
    [CampoSped(Ordem = 3)]
    public string? MsgRegra { get; set; }

    /// <summary>Registro onde ocorreu o aviso.</summary>
    [CampoSped(Ordem = 4, Tamanho = 4, Obrigatorio = true)]
    public string? Registro { get; set; }

    /// <summary>Nome do campo onde ocorreu o aviso.</summary>
    [CampoSped(Ordem = 5)]
    public string? Campo { get; set; }

    /// <summary>Conteúdo numérico preenchido pelo usuário.</summary>
    [CampoSped(Ordem = 6, Nome = "CONTEÚDO", Tamanho = 19, Decimais = 2)]
    public decimal? Conteudo { get; set; }

    /// <summary>Valor original ou calculado esperado.</summary>
    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2)]
    public decimal? ValorEsperado { get; set; }

    /// <summary>Período de apuração fiscal.</summary>
    [CampoSped(Ordem = 8, Tamanho = 3)]
    public string? PerApur { get; set; }

    /// <summary>Código da conta.</summary>
    [CampoSped(Ordem = 9)]
    public string? CodCta { get; set; }

    /// <summary>Código do centro de custos.</summary>
    [CampoSped(Ordem = 10)]
    public string? CodCcus { get; set; }

    /// <summary>Código da conta referencial.</summary>
    [CampoSped(Ordem = 11)]
    public string? CodCtaRef { get; set; }

    /// <summary>Código da linha na tabela dinâmica.</summary>
    [CampoSped(Ordem = 12, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Número de ordem do registro X300.</summary>
    [CampoSped(Ordem = 13)]
    public string? NumOrdem { get; set; }

    /// <summary>CNPJ do estabelecimento do registro Y540.</summary>
    [CampoSped(Ordem = 14, Tamanho = 14)]
    public Cnpj? CnpjEstab { get; set; }

    /// <summary>CNAE do registro Y540, preservado com zeros à esquerda.</summary>
    [CampoSped(Ordem = 15, Tamanho = 7)]
    public string? Cnae { get; set; }

    /// <summary>Código da conta da Parte B.</summary>
    [CampoSped(Ordem = 16)]
    public string? CodCtaB { get; set; }

    /// <summary>Tributo relacionado à conta da Parte B.</summary>
    [CampoSped(Ordem = 17)]
    public string? CodTributo { get; set; }
}
