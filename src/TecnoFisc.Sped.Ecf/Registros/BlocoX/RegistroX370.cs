using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecf.Versionamento;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X370 - informações sobre as transações controladas.</summary>
[RegistroSped(Codigo = "X370", Nivel = 2, Bloco = "X", IntroduzidoEm = (int)LayoutEcf.V010)]
public sealed partial class RegistroX370 : RegistroSped
{
    public override string Codigo => "X370";

    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true, Nome = "IDENTIFICADOR")]
    public string? Identificador { get; set; }

    /// <summary>Tipo de transação preservado como código lexical de domínio do leiaute.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true, Nome = "TIPO_TRANSACAO")]
    public string? TipoTransacao { get; set; }

    [CampoSped(Ordem = 4, Obrigatorio = true, Nome = "NOME_ENT")]
    public string? NomeEnt { get; set; }

    /// <summary>País conforme a tabela dinâmica PAIS_SISCOMEX.</summary>
    [CampoSped(Ordem = 5, Obrigatorio = true, Nome = "PAIS")]
    public string? Pais { get; set; }

    /// <summary>NCM conforme tabela dinâmica, preservado lexicalmente.</summary>
    [CampoSped(Ordem = 6, Nome = "COD_NCM")]
    public string? CodNcm { get; set; }

    /// <summary>Tipo de serviço, direito ou operação, preservado como código lexical.</summary>
    [CampoSped(Ordem = 7, Tamanho = 3, Nome = "TIPO_DEMAIS")]
    public string? TipoDemais { get; set; }

    [CampoSped(Ordem = 8, Obrigatorio = true, Nome = "DESCR_BSDI")]
    public string? DescrBsdi { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_TRANSACAO")]
    public decimal VlTransacao { get; set; }

    [CampoSped(Ordem = 10, Obrigatorio = true, Nome = "IND_AJUSTES")]
    public IndicadorSimNao IndAjustes { get; set; }

    /// <summary>Obrigatório de modo condicional quando há ajustes.</summary>
    [CampoSped(Ordem = 11, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_ESPONTANEO")]
    public decimal? VlEspontaneo { get; set; }

    /// <summary>Obrigatório de modo condicional quando há ajustes.</summary>
    [CampoSped(Ordem = 12, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_COMPENSATORIO")]
    public decimal? VlCompensatorio { get; set; }

    /// <summary>Tipo de ajuste compensatório preservado como código lexical.</summary>
    [CampoSped(Ordem = 13, Nome = "TIP_AJ_COMPENSATORIO")]
    public string? TipAjCompensatorio { get; set; }

    /// <summary>Método de preços de transferência conforme domínio do leiaute.</summary>
    [CampoSped(Ordem = 14, Tamanho = 3, Obrigatorio = true, Nome = "METODO")]
    public string? Metodo { get; set; }

    [CampoSped(Ordem = 15, Nome = "DESCRICAO")]
    public string? Descricao { get; set; }

    /// <summary>Indicador obrigatório de modo condicional ao método.</summary>
    [CampoSped(Ordem = 16, Tamanho = 1, Obrigatorio = true, Nome = "COMP_INTENCIONAL")]
    public IndicadorSimNao? CompIntencional { get; set; }

    /// <summary>Indicador obrigatório de modo condicional ao método.</summary>
    [CampoSped(Ordem = 17, Tamanho = 1, Obrigatorio = true, Nome = "SINERGIA")]
    public IndicadorSimNao? Sinergia { get; set; }

    /// <summary>Indicador obrigatório de modo condicional ao método.</summary>
    [CampoSped(Ordem = 18, Tamanho = 1, Obrigatorio = true, Nome = "IND_TRANS_COMBINADAS")]
    public IndicadorSimNao? IndTransCombinadas { get; set; }

    /// <summary>Indicador obrigatório de modo condicional ao método.</summary>
    [CampoSped(Ordem = 19, Tamanho = 1, Obrigatorio = true, Nome = "IND_DADOS_MULTIP")]
    public IndicadorSimNao? IndDadosMultip { get; set; }

    /// <summary>Indicador obrigatório apenas para as transações de serviços aplicáveis.</summary>
    [CampoSped(Ordem = 20, Tamanho = 1, Obrigatorio = true, Nome = "IND_SIMPLIFIC")]
    public IndicadorSimNao? IndSimplific { get; set; }
}
