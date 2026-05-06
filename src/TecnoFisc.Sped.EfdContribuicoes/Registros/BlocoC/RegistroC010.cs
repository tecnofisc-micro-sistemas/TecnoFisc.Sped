using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C010 — Identificação do Estabelecimento. Nível hierárquico 2, ocorrência vários por
/// arquivo. Escriturado apenas para estabelecimentos que realizaram aquisição, venda ou devolução
/// de mercadorias mediante emissão de documento fiscal do Bloco C. Conforme Guia Prático v1.35, p. 105.
/// </summary>
[RegistroSped(Codigo = "C010", Nivel = 2, Bloco = "C")]
public sealed partial class RegistroC010 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C010";

    /// <summary>
    /// CNPJ do estabelecimento (14 dígitos, sem máscara). Validação: DV conferido. O estabelecimento
    /// deve estar cadastrado no Registro 0140.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }

    /// <summary>
    /// Indicador da apuração das contribuições e créditos na escrituração das operações por NF-e e ECF.
    /// Obrigatório quando o arquivo contiver tanto registros individualizados quanto consolidados para
    /// o mesmo tipo de documento fiscal.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 1)]
    public IndicadorEscrituracaoNfe? IndEscri { get; set; }
}
