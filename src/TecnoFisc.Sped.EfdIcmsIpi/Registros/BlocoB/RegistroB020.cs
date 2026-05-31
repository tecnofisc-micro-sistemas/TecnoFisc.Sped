using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoB;

/// <summary>
/// Registro B020 — NF (01), NFS (03), NFS-Avulsa (3B), NF-Produtor (04), CT-Rod-Cargas (08),
/// NF-e (55), NFC-e (65). Nível hierárquico 2, ocorrência vários por arquivo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 45-46.
/// </summary>
/// <remarks>
/// <b>V016 (Guide 3.0.7 itens 19-20):</b> inclusão da NF3-e (modelo "66") na lista de
/// documentos válidos do campo 05 <c>COD_MOD</c>; validações atualizadas nos campos 04
/// <c>COD_PART</c>, 07 <c>SER</c> e 09 <c>CHV_NFE</c>.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </remarks>
[RegistroSped(Codigo = "B020", Nivel = 2, Bloco = "B")]
public sealed partial class RegistroB020 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "B020";

    /// <summary>Indicador do tipo de operação: 0 Aquisição, 1 Prestação.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacaoIss IndOper { get; set; }

    /// <summary>Indicador do emitente do documento fiscal: 0 Emissão própria, 1 Terceiros.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorEmissorDocumento IndEmit { get; set; }

    /// <summary>Código do participante (campo 02 do Registro 0150). Obrigatório para NFC-e.</summary>
    [CampoSped(Ordem = 4, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Código do modelo do documento fiscal conforme Tabela 4.1.3 (01, 03, 3B, 04, 08, 55, 65).</summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    /// <summary>Código da situação do documento conforme Tabela 4.1.2 (00, 02, 04, 05, 06, 08).</summary>
    [CampoSped(Ordem = 6, Tamanho = 2, Obrigatorio = true)]
    public CodigoSituacaoDocumentoFiscal CodSit { get; set; }

    /// <summary>Série do documento fiscal.</summary>
    [CampoSped(Ordem = 7, Tamanho = 3)]
    public string? Ser { get; set; }

    /// <summary>Número do documento fiscal.</summary>
    [CampoSped(Ordem = 8, Tamanho = 9, Obrigatorio = true)]
    public int? NumDoc { get; set; }

    /// <summary>Chave da Nota Fiscal Eletrônica (44 dígitos). Obrigatório para NF-e (55) e NFC-e (65) de emissão própria.</summary>
    [CampoSped(Ordem = 9, Tamanho = 44)]
    public ChaveAcesso? ChvNfe { get; set; }

    /// <summary>Data da emissão do documento fiscal no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 10, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Código do município onde o serviço foi prestado conforme tabela IBGE (7 dígitos).</summary>
    [CampoSped(Ordem = 11, Tamanho = 7, Obrigatorio = true)]
    public string? CodMunServ { get; set; }

    /// <summary>Valor contábil (valor total do documento).</summary>
    [CampoSped(Ordem = 12, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCont { get; set; }

    /// <summary>Valor do material fornecido por terceiros na prestação do serviço.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlMatTerc { get; set; }

    /// <summary>Valor da subempreitada.</summary>
    [CampoSped(Ordem = 14, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlSub { get; set; }

    /// <summary>Valor das operações isentas ou não tributadas pelo ISS.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIsntIss { get; set; }

    /// <summary>Valor da dedução da base de cálculo.</summary>
    [CampoSped(Ordem = 16, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDedBc { get; set; }

    /// <summary>Valor da base de cálculo do ISS.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIss { get; set; }

    /// <summary>Valor da base de cálculo de retenção do ISS.</summary>
    [CampoSped(Ordem = 18, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIssRt { get; set; }

    /// <summary>Valor do ISS retido pelo tomador.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIssRt { get; set; }

    /// <summary>Valor do ISS destacado.</summary>
    [CampoSped(Ordem = 20, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIss { get; set; }

    /// <summary>Código da observação do lançamento fiscal (campo 02 do Registro 0460).</summary>
    [CampoSped(Ordem = 21, Tamanho = 60)]
    public string? CodInfObs { get; set; }
}
