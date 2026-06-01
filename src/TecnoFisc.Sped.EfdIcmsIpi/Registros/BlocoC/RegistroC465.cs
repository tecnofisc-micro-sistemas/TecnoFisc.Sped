using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C465 — Complemento do Cupom Fiscal Eletrônico Emitido por ECF – CF-e-ECF (código 60).
/// Identifica adicionalmente os cupons fiscais eletrônicos emitidos por equipamentos ECF,
/// totalizados na Redução Z.
/// Nível hierárquico 5, ocorrência 1:1 por C460.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 126.
/// </summary>
[RegistroSped(Codigo = "C465", Nivel = 5, Bloco = "C")]
public sealed partial class RegistroC465 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C465";

    /// <summary>Chave do Cupom Fiscal Eletrônico (CF-e-ECF), 44 dígitos. O dígito verificador (DV) é conferido.</summary>
    [CampoSped(Ordem = 2, Tamanho = 44, Obrigatorio = true)]
    public string? ChvCfe { get; set; }

    /// <summary>Número do Contador de Cupom Fiscal. Deve ser maior que zero.</summary>
    [CampoSped(Ordem = 3, Tamanho = 9, Obrigatorio = true)]
    public int? NumCcf { get; set; }
}
