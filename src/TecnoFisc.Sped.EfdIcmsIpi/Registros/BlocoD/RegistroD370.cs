using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D370 — Complemento dos Documentos Informados (cód. 13, 14, 15, 16 e 2E).
/// Nível hierárquico 5, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 186.
/// </summary>
[RegistroSped(Codigo = "D370", Nivel = 5, Bloco = "D")]
public sealed partial class RegistroD370 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D370";

    /// <summary>Código do município de origem do serviço, conforme a tabela IBGE.</summary>
    [CampoSped(Ordem = 2, Tamanho = 7, Obrigatorio = true)]
    public string? CodMunOrig { get; set; }

    /// <summary>Valor total da prestação de serviço.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal? VlServ { get; set; }

    /// <summary>Quantidade de bilhetes emitidos.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true)]
    public int? QtdBilh { get; set; }

    /// <summary>Valor total da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor total do ICMS.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }
}
