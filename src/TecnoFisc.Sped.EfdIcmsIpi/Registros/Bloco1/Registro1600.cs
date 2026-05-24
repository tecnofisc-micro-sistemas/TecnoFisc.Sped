using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1600 — Total das operações com cartão de crédito e/ou débito, loja
/// (private label) e demais instrumentos de pagamentos eletrônicos.
/// Nível hierárquico 2, ocorrência 1:N. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 286.
/// </summary>
/// <remarks>
/// <b>Descontinuado a partir de V016</b> (Guia Prático 3.0.7, item 6). Substituído pelo
/// <see cref="Registro1601"/> para arquivos com <c>COD_VER ≥ 016</c>. Anotação
/// <see cref="DescontinuadoAttribute"/> é informacional no read path (ARCHITECTURE §4.7
/// read-only) — o parser continua reconhecendo o registro para que arquivos históricos
/// das versões anteriores sejam lidos sem erro de leiaute.
/// </remarks>
[RegistroSped(Codigo = "1600", Nivel = 2, Bloco = "1")]
[Descontinuado(EmVersao = (int)LayoutEfdIcmsIpi.V016)]
public sealed partial class Registro1600 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1600";

    /// <summary>Codigo do participante, campo 02 do Registro 0150, que identifica a instituicao financeira e/ou de pagamento.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodPart { get; set; }

    /// <summary>Valor total das operacoes de credito realizadas no periodo.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal TotCredito { get; set; }

    /// <summary>Valor total das operacoes de debito realizadas no periodo.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal TotDebito { get; set; }
}
