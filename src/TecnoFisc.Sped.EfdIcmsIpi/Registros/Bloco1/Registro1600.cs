using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1600 - Total das operacoes com cartao de credito e/ou debito, loja
/// (private label) e demais instrumentos de pagamentos eletronicos.
/// Nivel hierarquico 2, ocorrencia 1:N. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 286.
/// </summary>
[RegistroSped(Codigo = "1600", Nivel = 2, Bloco = "1")]
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
