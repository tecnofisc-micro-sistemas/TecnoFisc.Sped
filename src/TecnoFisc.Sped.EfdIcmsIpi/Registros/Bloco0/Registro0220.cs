using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0220 — Fatores de Conversão de Unidades. Nível hierárquico 3, ocorrência 1:N por
/// Registro0200. Informa o fator de conversão entre a unidade comercial do item (UNID_CONV) e
/// a unidade de inventário. Conforme Guia Prático EFD-ICMS/IPI v3.2.2, p. 41.
/// </summary>
[RegistroSped(Codigo = "0220", Nivel = 3, Bloco = "0")]
public sealed partial class Registro0220 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0220";

    /// <summary>Unidade comercial a ser convertida na unidade de estoque, referida no registro 0200.</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? UnidConv { get; set; }

    /// <summary>Fator de conversão utilizado para converter (multiplicar) a unidade a ser convertida na unidade adotada no inventário. Deve ser maior que zero.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal FatConv { get; set; }

    /// <summary>Representação alfanumérica do código de barra (GTIN-8, GTIN-12, GTIN-13 ou GTIN-14) da unidade comercial do produto, se houver.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, DesdeVersao = (int)LayoutEfdIcmsIpi.V016)]
    public string? CodBarra { get; set; }
}
