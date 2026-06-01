using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0221 — Correlação entre códigos de itens comercializados. Informa a relação entre os
/// diversos códigos de item (relacionados a uma mesma mercadoria) utilizados nos documentos
/// fiscais e nos registros da EFD-ICMS/IPI, sempre em referência ao item "atômico" (menor unidade
/// de comercialização praticada pelo estabelecimento). Filho do <see cref="Registro0200"/> quando
/// o campo <c>TIPO_ITEM</c> deste estiver informado com valor "00 — Mercadoria para Revenda".
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 42 (Subseção 12).
/// </summary>
/// <remarks>
/// <b>V017 (Guide 3.1.0):</b> registro introduzido com 2 campos. Obrigatoriedade pode ser
/// estabelecida a partir de 2024 e é definida pela UF de domicílio do declarante.
/// <para>
/// <b>Validação do leiaute (não aplicada aqui):</b> não podem ser informados dois ou mais
/// registros, filhos do mesmo 0200 pai, com o mesmo conteúdo no campo <c>COD_ITEM_ATOMICO</c>.
/// Quando <c>COD_ITEM_ATOMICO</c> for igual ao <c>COD_ITEM</c> do 0200 pai, <c>QTD_CONTIDA</c>
/// deverá ser igual a "1". Como o pacote é read-only, estas regras ficam a cargo do consumidor —
/// o leitor preserva todos os registros como vieram no arquivo.
/// </para>
/// <para>
/// Pacote read-only — modelo único do leiaute mais recente; atributos <see cref="CampoSpedAttribute.DesdeVersao"/>
/// e <see cref="RegistroSpedAttribute.IntroduzidoEm"/> são informacionais (ver ARCHITECTURE §2.5 / §4.7).
/// </para>
/// </remarks>
[RegistroSped(Codigo = "0221", Nivel = 3, Bloco = "0", IntroduzidoEm = (int)LayoutEfdIcmsIpi.V017)]
public sealed partial class Registro0221 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0221";

    /// <summary>Código do item atômico contido no item informado no <see cref="Registro0200"/> pai.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public string? CodItemAtomico { get; set; }

    /// <summary>Quantos itens atômicos estão contidos no item informado no <see cref="Registro0200"/> pai. Deve ser maior ou igual a 1.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 6, Obrigatorio = true, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public decimal? QtdContida { get; set; }
}
