using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoA;

/// <summary>
/// Registro A010 — Identificação do Estabelecimento. Nível hierárquico 2, ocorrência vários
/// (por arquivo). Deve ser escriturado apenas para estabelecimentos que efetivamente realizaram
/// operações de prestação ou contratação de serviços mediante emissão de documento fiscal no
/// período. Conforme Guia Prático v1.35, p. 95.
/// </summary>
[RegistroSped(Codigo = "A010", Nivel = 2, Bloco = "A")]
public sealed partial class RegistroA010 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "A010";

    /// <summary>
    /// CNPJ do estabelecimento (14 dígitos, sem máscara). Validação: DV conferido. O estabelecimento
    /// deve estar cadastrado no Registro 0140.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }
}
