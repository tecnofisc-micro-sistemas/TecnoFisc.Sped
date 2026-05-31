using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoP;

/// <summary>
/// Registro P110 — Complemento da Escrituração – Detalhamento da Apuração da Contribuição.
/// Nível hierárquico 4, ocorrência 1:N (filho de P100). Detalha analiticamente os valores
/// consolidados no Registro P100 com base na Tabela 5.1.2. Conforme Guia Prático v1.35, p. 369.
/// </summary>
[RegistroSped(Codigo = "P110", Nivel = 4, Bloco = "P")]
public sealed partial class RegistroP110 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "P110";

    /// <summary>Número do campo do registro P100 objeto de detalhamento (2 caracteres fixos).</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? NumCampo { get; set; }

    /// <summary>Código do tipo de detalhamento, conforme Tabela 5.1.2 (8 caracteres fixos).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8)]
    public string? CodDet { get; set; }

    /// <summary>
    /// Valor detalhado referente ao campo indicado por NUM_CAMPO. A soma de todos os valores
    /// deste campo nos registros P110 não pode ser maior que o valor constante no campo P100
    /// a que se refere o Campo 02 do registro P110.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal DetValor { get; set; }

    /// <summary>Informação complementar do detalhamento.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? InfCompl { get; set; }
}
