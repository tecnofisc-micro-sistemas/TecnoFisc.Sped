using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1050 — Detalhamento de Ajustes de Base de Cálculo – Valores Extra Apuração.
/// Nível hierárquico 2, ocorrência 1:N. Detalha totais de valores extra apuração de ajuste
/// da base de cálculo mensal da contribuição informados no Bloco M, segregados por CST e CNPJ.
/// Conforme Guia Prático EFD Contribuições v1.35, p. 382.
/// </summary>
[RegistroSped(Codigo = "1050", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1050 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1050";

    /// <summary>Data de referência do ajuste no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtRef { get; set; }

    /// <summary>Indicador da natureza do ajuste da base de cálculo, conforme Tabela Externa 4.3.18.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? IndAjBc { get; set; }

    /// <summary>CNPJ do estabelecimento a que se refere o ajuste.</summary>
    [CampoSped(Ordem = 4, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }

    /// <summary>Valor total do ajuste.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjTot { get; set; }

    /// <summary>Parcela do ajuste na base de cálculo referente ao CST 01.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjCst01 { get; set; }

    /// <summary>Parcela do ajuste na base de cálculo referente ao CST 02.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjCst02 { get; set; }

    /// <summary>Parcela do ajuste na base de cálculo referente ao CST 03.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjCst03 { get; set; }

    /// <summary>Parcela do ajuste na base de cálculo referente ao CST 04.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjCst04 { get; set; }

    /// <summary>Parcela do ajuste na base de cálculo referente ao CST 05.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjCst05 { get; set; }

    /// <summary>Parcela do ajuste na base de cálculo referente ao CST 06.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjCst06 { get; set; }

    /// <summary>Parcela do ajuste na base de cálculo referente ao CST 07.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjCst07 { get; set; }

    /// <summary>Parcela do ajuste na base de cálculo referente ao CST 08.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjCst08 { get; set; }

    /// <summary>Parcela do ajuste na base de cálculo referente ao CST 09.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjCst09 { get; set; }

    /// <summary>Parcela do ajuste na base de cálculo referente ao CST 49.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjCst49 { get; set; }

    /// <summary>Parcela do ajuste na base de cálculo referente ao CST 99.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjCst99 { get; set; }

    /// <summary>Indicador de apropriação do ajuste (PIS/Pasep, Cofins ou ambos).</summary>
    [CampoSped(Ordem = 17, Tamanho = 2, Obrigatorio = true)]
    public IndicadorApropriacaoContribuicao IndAprop { get; set; }

    /// <summary>Número do recibo da escrituração a que se refere o ajuste.</summary>
    [CampoSped(Ordem = 18, Tamanho = 80)]
    public string? NumRec { get; set; }

    /// <summary>Informação complementar do registro.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0)]
    public string? InfoCompl { get; set; }
}
