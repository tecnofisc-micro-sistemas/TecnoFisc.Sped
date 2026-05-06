using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco1;

/// <summary>
/// Registro 1011 — Detalhamento das Contribuições com Exigibilidade Suspensa.
/// Nível hierárquico 3, ocorrência 1:N (filho de Registro 1010). Detalha os valores de
/// PIS/Pasep e Cofins com exigibilidade suspensa por decisão judicial, segregados por CST.
/// Disponível a partir do leiaute VI (janeiro/2020). Conforme Guia Prático v1.35, p. 377.
/// </summary>
[RegistroSped(Codigo = "1011", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1011 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1011";

    /// <summary>Registro da escrituração com detalhamento das contribuições (ex.: "C170", "A100"). 4 caracteres fixos.</summary>
    [CampoSped(Ordem = 2, Tamanho = 4)]
    public string? RegRef { get; set; }

    /// <summary>Chave do documento eletrônico referenciado (até 90 caracteres).</summary>
    [CampoSped(Ordem = 3, Tamanho = 90)]
    public string? ChaveDoc { get; set; }

    /// <summary>Código do participante conforme Registro 0150, Campo 02 (até 60 caracteres).</summary>
    [CampoSped(Ordem = 4, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Código do item conforme Registro 0200, Campo 02 (até 60 caracteres).</summary>
    [CampoSped(Ordem = 5, Tamanho = 60)]
    public string? CodItem { get; set; }

    /// <summary>Data da operação no formato DDMMAAAA.</summary>
    [CampoSped(Ordem = 6, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtOper { get; set; }

    /// <summary>Valor da operação/item (base para a contribuição suspensa). 2 casas decimais.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOper { get; set; }

    /// <summary>CST do PIS/Pasep conforme escrituração, conforme Tabela item 4.3.3. 2 dígitos.</summary>
    [CampoSped(Ordem = 8, Tamanho = 2, Obrigatorio = true)]
    public int CstPis { get; set; }

    /// <summary>Base de cálculo do PIS/Pasep conforme escrituração. 4 casas decimais.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 4)]
    public decimal? VlBcPis { get; set; }

    /// <summary>Alíquota do PIS/Pasep conforme escrituração. 4 casas decimais.</summary>
    [CampoSped(Ordem = 10, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPis { get; set; }

    /// <summary>Valor do PIS/Pasep conforme escrituração. 2 casas decimais.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>CST da Cofins conforme escrituração, conforme Tabela item 4.3.4. 2 dígitos.</summary>
    [CampoSped(Ordem = 12, Tamanho = 2, Obrigatorio = true)]
    public int CstCofins { get; set; }

    /// <summary>Base de cálculo da Cofins conforme escrituração. 4 casas decimais.</summary>
    [CampoSped(Ordem = 13, Tamanho = 0, Decimais = 4)]
    public decimal? VlBcCofins { get; set; }

    /// <summary>Alíquota da Cofins conforme escrituração. 4 casas decimais.</summary>
    [CampoSped(Ordem = 14, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofins { get; set; }

    /// <summary>Valor da Cofins conforme escrituração. 2 casas decimais.</summary>
    [CampoSped(Ordem = 15, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }

    /// <summary>CST do PIS/Pasep conforme decisão judicial, conforme Tabela item 4.3.3. 2 dígitos.</summary>
    [CampoSped(Ordem = 16, Tamanho = 2, Obrigatorio = true)]
    public int CstPisSusp { get; set; }

    /// <summary>Base de cálculo do PIS/Pasep conforme decisão judicial. 4 casas decimais.</summary>
    [CampoSped(Ordem = 17, Tamanho = 0, Decimais = 4)]
    public decimal? VlBcPisSusp { get; set; }

    /// <summary>Alíquota do PIS/Pasep conforme decisão judicial. 4 casas decimais.</summary>
    [CampoSped(Ordem = 18, Tamanho = 8, Decimais = 4)]
    public decimal? AliqPisSusp { get; set; }

    /// <summary>Valor do PIS/Pasep conforme decisão judicial. 2 casas decimais.</summary>
    [CampoSped(Ordem = 19, Tamanho = 0, Decimais = 2)]
    public decimal? VlPisSusp { get; set; }

    /// <summary>CST da Cofins conforme decisão judicial, conforme Tabela item 4.3.4. 2 dígitos.</summary>
    [CampoSped(Ordem = 20, Tamanho = 2, Obrigatorio = true)]
    public int CstCofinsSusp { get; set; }

    /// <summary>Base de cálculo da Cofins conforme decisão judicial. 4 casas decimais.</summary>
    [CampoSped(Ordem = 21, Tamanho = 0, Decimais = 4)]
    public decimal? VlBcCofinsSusp { get; set; }

    /// <summary>Alíquota da Cofins conforme decisão judicial. 4 casas decimais.</summary>
    [CampoSped(Ordem = 22, Tamanho = 8, Decimais = 4)]
    public decimal? AliqCofinsSusp { get; set; }

    /// <summary>Valor da Cofins conforme decisão judicial. 2 casas decimais.</summary>
    [CampoSped(Ordem = 23, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofinsSusp { get; set; }

    /// <summary>Código da conta analítica contábil debitada/creditada (até 255 caracteres).</summary>
    [CampoSped(Ordem = 24, Tamanho = 255)]
    public string? CodCta { get; set; }

    /// <summary>Código do centro de custos (até 255 caracteres).</summary>
    [CampoSped(Ordem = 25, Tamanho = 255)]
    public string? CodCcus { get; set; }

    /// <summary>Descrição complementar do documento/operação.</summary>
    [CampoSped(Ordem = 26, Tamanho = 0)]
    public string? DescDocOper { get; set; }
}
