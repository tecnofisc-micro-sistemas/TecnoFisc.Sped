using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y620 - participações avaliadas pelo método de equivalência patrimonial.</summary>
[RegistroSped(Codigo = "Y620", Nivel = 2, Bloco = "Y")]
public sealed partial class RegistroY620 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y620";

    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "DT_EVENTO")]
    public DateOnly DtEvento { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true, Nome = "IND_RELAC")]
    public TipoRelacionamentoParticipacao IndRelac { get; set; }

    /// <summary>Código de país preservado lexicalmente.</summary>
    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true, Nome = "PAIS")]
    public string? Pais { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 14, Nome = "CNPJ")]
    public Cnpj? Cnpj { get; set; }

    [CampoSped(Ordem = 6, Obrigatorio = true, Nome = "NOM_EMP")]
    public string? NomEmp { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VALOR_REAIS")]
    public decimal ValorReais { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VALOR_ESTR")]
    public decimal ValorEstr { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 8, Decimais = 4, Obrigatorio = true, Nome = "PERC_CAP_TOT")]
    public decimal PercCapTot { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 8, Decimais = 4, Obrigatorio = true, Nome = "PERC_CAP_VOT")]
    public decimal PercCapVot { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 19, Decimais = 2, Nome = "RES_EQ_PAT")]
    public decimal? ResEqPat { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "DATA_AQUIS")]
    public DateOnly DataAquis { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 1, Obrigatorio = true, Nome = "IND_PROC_CART")]
    public IndicadorSimNao IndProcCart { get; set; }

    [CampoSped(Ordem = 14, Nome = "NUM_PROC_CART")]
    public string? NumProcCart { get; set; }

    [CampoSped(Ordem = 15, Nome = "NOME_CART")]
    public string? NomeCart { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 1, Obrigatorio = true, Nome = "IND_PROC_RFB")]
    public IndicadorSimNao IndProcRfb { get; set; }

    [CampoSped(Ordem = 17, Nome = "NUM_PROC_RFB")]
    public string? NumProcRfb { get; set; }
}
