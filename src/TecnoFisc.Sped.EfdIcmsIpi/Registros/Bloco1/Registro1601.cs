using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1601 — Operações com instrumentos de pagamentos eletrônicos.
/// Nível hierárquico 2, ocorrência 1:N, OC. Vigente a partir de 01/01/2022 (V016).
/// Substitui o Registro 1600. Conforme Guia Prático EFD-ICMS/IPI v3.2.2, p. 305.
/// </summary>
[RegistroSped(Codigo = "1601", Nivel = 2, Bloco = "1", IntroduzidoEm = (int)LayoutEfdIcmsIpi.V016)]
public sealed partial class Registro1601 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1601";

    /// <summary>Código do participante (campo 02 do Registro 0150): identificação da instituição que efetuou o pagamento.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodPartIp { get; set; }

    /// <summary>Código do participante (campo 02 do Registro 0150): identificação do intermediador da transação.</summary>
    [CampoSped(Ordem = 3, Tamanho = 60)]
    public string? CodPartIt { get; set; }

    /// <summary>Valor total bruto das vendas e/ou prestações de serviços no campo de incidência do ICMS, incluindo operações com imunidade do imposto.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal TotVs { get; set; }

    /// <summary>Valor total bruto das prestações de serviços no campo de incidência do ISS.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal TotIss { get; set; }

    /// <summary>Valor total de operações deduzido dos valores dos campos TOT_VS e TOT_ISS.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal TotOutros { get; set; }
}
