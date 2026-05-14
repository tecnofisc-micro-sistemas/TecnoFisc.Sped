using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1010 - Obrigatoriedade de Registros do Bloco 1. Nivel hierarquico 2,
/// ocorrencia unica por arquivo. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 268-269.
/// </summary>
[RegistroSped(Codigo = "1010", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1010 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1010";

    /// <summary>Reg. 1100 - ocorreu averbacao de exportacao no periodo.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndExp { get; set; }

    /// <summary>Reg. 1200 - existem informacoes acerca de creditos de ICMS a controlar.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndCcrf { get; set; }

    /// <summary>Reg. 1300 - comercio varejista de combustiveis com movimentacao ou estoque.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndComb { get; set; }

    /// <summary>Reg. 1390 - produtor de acucar ou alcool carburante com movimentacao ou estoque.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndUsina { get; set; }

    /// <summary>Reg. 1400 - existem informacoes sobre valor agregado a prestar.</summary>
    [CampoSped(Ordem = 6, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndVa { get; set; }

    /// <summary>Reg. 1500 - distribuidora de energia com fornecimento para consumidores de outra UF.</summary>
    [CampoSped(Ordem = 7, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndEe { get; set; }

    /// <summary>Reg. 1600 - realizou vendas com cartao de credito ou debito.</summary>
    [CampoSped(Ordem = 8, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndCart { get; set; }

    /// <summary>Reg. 1700 - emitiu documentos fiscais em papel no periodo.</summary>
    [CampoSped(Ordem = 9, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndForm { get; set; }

    /// <summary>Reg. 1800 - prestou servicos de transporte aereo de cargas e passageiros.</summary>
    [CampoSped(Ordem = 10, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndAer { get; set; }

    /// <summary>Reg. 1960 - possui informacoes GIAF1.</summary>
    [CampoSped(Ordem = 11, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndGiaf1 { get; set; }

    /// <summary>Reg. 1970 - possui informacoes GIAF3.</summary>
    [CampoSped(Ordem = 12, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndGiaf3 { get; set; }

    /// <summary>Reg. 1980 - possui informacoes GIAF4.</summary>
    [CampoSped(Ordem = 13, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndGiaf4 { get; set; }

    /// <summary>Reg. 1250 - possui saldos de restituicao, ressarcimento e complementacao do ICMS.</summary>
    [CampoSped(Ordem = 14, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndRestRessarcComplIcms { get; set; }
}
