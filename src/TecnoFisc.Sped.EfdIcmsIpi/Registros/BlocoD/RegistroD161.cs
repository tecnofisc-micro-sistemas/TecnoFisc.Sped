using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D161 — Local da Coleta e Entrega (Códigos 08, 8B, 09, 10, 11 e 26).
/// Nível hierárquico 4, ocorrência 1:1.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 174.
/// </summary>
[RegistroSped(Codigo = "D161", Nivel = 4, Bloco = "D")]
public sealed partial class RegistroD161 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D161";

    /// <summary>Indicador do tipo de transporte da carga coletada.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1)]
    public IndicadorTipoCarga? IndCarga { get; set; }

    /// <summary>CNPJ ou CPF do contribuinte do local da coleta (14 dígitos CNPJ ou 11 dígitos CPF).</summary>
    [CampoSped(Ordem = 3, Tamanho = 14)]
    public string? CnpjCpfCol { get; set; }

    /// <summary>Inscrição Estadual do contribuinte do local de coleta.</summary>
    [CampoSped(Ordem = 4, Tamanho = 14)]
    public string? IeCol { get; set; }

    /// <summary>Código do Município do local de coleta, conforme tabela IBGE (9999999 para Exterior).</summary>
    [CampoSped(Ordem = 5, Tamanho = 7)]
    public int? CodMunCol { get; set; }

    /// <summary>CNPJ ou CPF do contribuinte do local da entrega (14 dígitos CNPJ ou 11 dígitos CPF).</summary>
    [CampoSped(Ordem = 6, Tamanho = 14)]
    public string? CnpjCpfEntg { get; set; }

    /// <summary>Inscrição Estadual do contribuinte do local de entrega.</summary>
    [CampoSped(Ordem = 7, Tamanho = 14)]
    public string? IeEntg { get; set; }

    /// <summary>Código do Município do local de entrega, conforme tabela IBGE (9999999 para Exterior).</summary>
    [CampoSped(Ordem = 8, Tamanho = 7)]
    public int? CodMunEntg { get; set; }
}
