using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1105 - Documentos Fiscais de Exportacao. Nivel hierarquico 3,
/// ocorrencia varios por Registro 1100. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 271.
/// </summary>
[RegistroSped(Codigo = "1105", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1105 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1105";

    /// <summary>Codigo do modelo da NF, conforme Tabela 4.1.1.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public ModeloDocumento CodMod { get; set; }

    /// <summary>Serie da Nota Fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 3)]
    public string? Serie { get; set; }

    /// <summary>Numero de Nota Fiscal de exportacao emitida pelo exportador.</summary>
    [CampoSped(Ordem = 4, Tamanho = 9, Obrigatorio = true)]
    public int NumDoc { get; set; }

    /// <summary>Chave da Nota Fiscal Eletronica.</summary>
    [CampoSped(Ordem = 5, Tamanho = 44)]
    public ChaveAcesso? ChvNfe { get; set; }

    /// <summary>Data da emissao da NF de exportacao no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 6, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDoc { get; set; }

    /// <summary>Codigo do item constante do Registro 0200.</summary>
    [CampoSped(Ordem = 7, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }
}
