using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoD;

/// <summary>
/// Registro D010 — Identificação do Estabelecimento. Nível hierárquico 2, ocorrência vários
/// por arquivo. Escriturado apenas para estabelecimentos que efetivamente realizaram prestação ou
/// contratação de serviços de transporte (carga, passagem, comunicação e telecomunicação) mediante
/// emissão de documento fiscal do Bloco D. Conforme Guia Prático v1.35, p. 193.
/// </summary>
[RegistroSped(Codigo = "D010", Nivel = 2, Bloco = "D")]
public sealed partial class RegistroD010 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D010";

    /// <summary>
    /// CNPJ do estabelecimento (14 dígitos, sem máscara). Validação: DV conferido. O estabelecimento
    /// deve estar cadastrado no Registro 0140.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }
}
