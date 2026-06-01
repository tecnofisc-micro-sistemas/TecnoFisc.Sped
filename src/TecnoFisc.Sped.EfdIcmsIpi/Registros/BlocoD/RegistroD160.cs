using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D160 — Carga Transportada (Códigos 08, 8B, 09, 10, 11, 26 e 27).
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 180.
/// </summary>
[RegistroSped(Codigo = "D160", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD160 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D160";

    /// <summary>Identificação do número do despacho (transporte ferroviário de cargas).</summary>
    [CampoSped(Ordem = 2, Tamanho = 0)]
    public string? Despacho { get; set; }

    /// <summary>CNPJ ou CPF do remetente das mercadorias que constam na nota fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? CnpjCpfRem { get; set; }

    /// <summary>Inscrição Estadual do remetente das mercadorias que constam na nota fiscal.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? IeRem { get; set; }

    /// <summary>Código do Município de origem, conforme tabela IBGE (9999999 para Exterior).</summary>
    [CampoSped(Ordem = 5, Tamanho = 7)]
    public int? CodMunOri { get; set; }

    /// <summary>CNPJ ou CPF do destinatário das mercadorias que constam na nota fiscal.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0)]
    public string? CnpjCpfDest { get; set; }

    /// <summary>Inscrição Estadual do destinatário das mercadorias que constam na nota fiscal.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0)]
    public string? IeDest { get; set; }

    /// <summary>Código do Município de destino, conforme tabela IBGE (9999999 para Exterior).</summary>
    [CampoSped(Ordem = 8, Tamanho = 7)]
    public int? CodMunDest { get; set; }
}
