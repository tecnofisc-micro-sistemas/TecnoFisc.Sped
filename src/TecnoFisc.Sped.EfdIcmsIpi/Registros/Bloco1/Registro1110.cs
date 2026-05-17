using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1110 - Operacoes de Exportacao Indireta - Mercadorias de Terceiros.
/// Nivel hierarquico 4, ocorrencia varios por Registro 1105.
/// Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 271-272.
/// </summary>
[RegistroSped(Codigo = "1110", Nivel = 4, Bloco = "1")]
public sealed partial class Registro1110 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1110";

    /// <summary>Codigo do participante fornecedor da mercadoria destinada a exportacao.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodPart { get; set; }

    /// <summary>Codigo do modelo do documento fiscal recebido, conforme Tabela 4.1.1.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public ModeloDocumento CodMod { get; set; }

    /// <summary>Serie do documento fiscal recebido com fins especificos de exportacao.</summary>
    [CampoSped(Ordem = 4, Tamanho = 4)]
    public string? Ser { get; set; }

    /// <summary>Numero do documento fiscal recebido com fins especificos de exportacao.</summary>
    [CampoSped(Ordem = 5, Tamanho = 9, Obrigatorio = true)]
    public int NumDoc { get; set; }

    /// <summary>Data da emissao do documento fiscal no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 6, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Chave da Nota Fiscal Eletronica emitida pelo participante para o exportador.</summary>
    [CampoSped(Ordem = 7, Tamanho = 44)]
    public ChaveAcesso? ChvNfe { get; set; }

    /// <summary>Numero do memorando de exportacao, quando houver.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0)]
    public long? NrMemo { get; set; }

    /// <summary>Quantidade do item efetivamente exportado.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal Qtd { get; set; }

    /// <summary>Unidade do item constante do Registro 0190.</summary>
    [CampoSped(Ordem = 10, Tamanho = 6, Obrigatorio = true)]
    public string? Unid { get; set; }
}
