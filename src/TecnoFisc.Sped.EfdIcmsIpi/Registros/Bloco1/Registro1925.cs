using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1925 - Informacoes adicionais da sub-apuracao - valores declaratorios.
/// Nivel hierarquico 5, ocorrencia 1:N. Conforme Guia Pratico EFD-ICMS/IPI
/// V3.0.6, p. 295.
/// </summary>
[RegistroSped(Codigo = "1925", Nivel = 5, Bloco = "1")]
public sealed partial class Registro1925 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1925";

    /// <summary>Codigo da informacao adicional conforme tabela definida pelas SEFAZ.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Obrigatorio = true)]
    public string? CodInfAdic { get; set; }

    /// <summary>Valor referente a informacao adicional.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlInfAdic { get; set; }

    /// <summary>Descricao complementar do ajuste.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? DescrComplAj { get; set; }
}
