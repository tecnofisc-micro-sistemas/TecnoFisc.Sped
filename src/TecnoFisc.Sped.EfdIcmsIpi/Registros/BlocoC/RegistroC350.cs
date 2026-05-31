using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C350 — Nota Fiscal de Venda a Consumidor (código 02).
/// Apresentado pelos contribuintes que utilizam notas fiscais de venda ao consumidor
/// não emitidas por ECF. Notas canceladas não devem ser informadas.
/// Nível hierárquico 2, ocorrência vários por arquivo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 113.
/// </summary>
[RegistroSped(Codigo = "C350", Nivel = 2, Bloco = "C")]
public sealed partial class RegistroC350 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C350";

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3)]
    public string? Ser { get; set; }

    /// <summary>Subsérie do documento fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 3)]
    public string? SubSer { get; set; }

    /// <summary>Número do documento fiscal.</summary>
    [CampoSped(Ordem = 4, Tamanho = 6, Obrigatorio = true)]
    public int? NumDoc { get; set; }

    /// <summary>Data da emissão do documento fiscal no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 5, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>CNPJ ou CPF do destinatário. Validado como CNPJ se 14 dígitos, como CPF se 11 dígitos.</summary>
    [CampoSped(Ordem = 6, Tamanho = 14)]
    public string? CnpjCpf { get; set; }

    /// <summary>Valor das mercadorias constantes no documento fiscal.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlMerc { get; set; }

    /// <summary>Valor total do documento fiscal.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    /// <summary>Valor total do desconto.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? VlDesc { get; set; }

    /// <summary>Valor total do PIS. Dispensado para contribuintes que entregam EFD-Contribuições.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor total da COFINS. Dispensado para contribuintes que entregam EFD-Contribuições.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0)]
    public string? CodCta { get; set; }
}
