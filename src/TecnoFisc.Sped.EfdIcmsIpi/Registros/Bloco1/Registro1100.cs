using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1100 - Registro de Informacoes sobre Exportacao. Nivel hierarquico 2,
/// ocorrencia varios por arquivo. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 269-270.
/// </summary>
[RegistroSped(Codigo = "1100", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1100";

    /// <summary>Tipo de documento de exportacao: 0-DE, 1-DSE, 2-DU-E.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDocumentoExportacao IndDoc { get; set; }

    /// <summary>Numero da declaracao de exportacao, sem mascara.</summary>
    [CampoSped(Ordem = 3, Tamanho = 14, Obrigatorio = true)]
    public string? NroDe { get; set; }

    /// <summary>Data da declaracao no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDe { get; set; }

    /// <summary>Natureza da exportacao: 0-direta, 1-indireta.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public NaturezaExportacao NatExp { get; set; }

    /// <summary>Numero do Registro de Exportacao, obrigatorio quando IND_DOC = 0.</summary>
    [CampoSped(Ordem = 6, Tamanho = 12)]
    public long? NroRe { get; set; }

    /// <summary>Data do Registro de Exportacao no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtRe { get; set; }

    /// <summary>Numero do conhecimento de embarque.</summary>
    [CampoSped(Ordem = 8, Tamanho = 18)]
    public string? ChcEmb { get; set; }

    /// <summary>Data do conhecimento de embarque no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 9, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtChc { get; set; }

    /// <summary>Data da averbacao da declaracao de exportacao no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 10, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtAvb { get; set; }

    /// <summary>Tipo de conhecimento de embarque.</summary>
    [CampoSped(Ordem = 11, Tamanho = 2, Obrigatorio = true)]
    public TipoConhecimentoEmbarque TpChc { get; set; }

    /// <summary>Codigo do pais de destino da mercadoria, conforme tabela SISCOMEX.</summary>
    [CampoSped(Ordem = 12, Tamanho = 3, Obrigatorio = true)]
    public string? Pais { get; set; }
}
