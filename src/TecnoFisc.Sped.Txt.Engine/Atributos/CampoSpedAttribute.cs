namespace TecnoFisc.Sped.Txt.Engine.Atributos;

/// <summary>
/// Marca uma propriedade como um campo persistido em um registro SPED. A ordem espelha a
/// numeração "Nº" das tabelas do Guia Prático: REG é o campo Nº 1 (não recebe atributo,
/// é resolvido pelo próprio código do registro) e os demais começam em <c>Ordem = 2</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CampoSpedAttribute : Attribute
{
    /// <summary>
    /// Nome normativo opcional do campo no leiaute. Quando nulo ou vazio, o catálogo mantém o
    /// nome da propriedade CLR. Use o alias somente quando o nome normativo colidir com um
    /// membro do modelo, por exemplo <c>CampoCodigo</c> com <c>Nome = "CODIGO"</c>.
    /// </summary>
    /// <remarks>
    /// Um alias não vazio deve ser um identificador ASCII sem espaços: letra ou sublinhado no
    /// início, seguido apenas de letras, dígitos ou sublinhados. Whitespace é inválido.
    /// </remarks>
    public string? Nome { get; init; }

    /// <summary>
    /// Posição do campo no layout, idêntica à coluna "Nº" do Guia Prático. REG ocupa a
    /// posição 1 (implícita); o primeiro campo declarado em código começa em 2.
    /// </summary>
    public required int Ordem { get; init; }

    /// <summary>Tamanho máximo declarado pelo layout. <c>0</c> indica tamanho livre.</summary>
    public int Tamanho { get; init; }

    /// <summary>Quantidade de casas decimais para campos numéricos. <c>0</c> para inteiros e textos.</summary>
    public int Decimais { get; init; }

    /// <summary>Indica se o layout exige preenchimento; falso permite valor vazio.</summary>
    public bool Obrigatorio { get; init; }

    /// <summary>
    /// Formato textual auxiliar para datas e similares. Para datas use "ddMMyyyy" (padrão SPED)
    /// ou "MMyyyy" para campos de período. Outros tipos ignoram.
    /// </summary>
    public string? Formato { get; init; }

    /// <summary>
    /// Versão do leiaute em que o campo passou a existir. Convenção: valor numérico do enum
    /// <c>LayoutXxx</c> do módulo (ex.: <c>(int)LayoutEfdIcmsIpi.V015</c> = 15). O parser/gerador
    /// usa este valor para incluir o campo somente quando a versão lida do <c>Registro0000</c>
    /// for maior ou igual. <c>0</c> (default) significa "presente em todas as versões".
    /// </summary>
    /// <remarks>
    /// Como SPED é strict-incremental por convenção da Receita, não há contraparte
    /// <c>AteVersao</c> — campos não são removidos em versões posteriores. Caso a Receita quebre
    /// essa regra no futuro, a estratégia documentada em <c>ARCHITECTURE.md §4.7</c> é
    /// subclasse de registro (<c>RegistroXxxxVYYY : RegistroXxxx</c>) em vez de extender este
    /// atributo.
    /// </remarks>
    public int DesdeVersao { get; init; }

    /// <summary>
    /// Quando <c>true</c>, indica que este campo variádico (<c>*</c> nas tabelas do guia)
    /// deve capturar tudo que restar na linha a partir da sua posição, incluindo os separadores
    /// <c>|</c> intermediários, como uma única string pipe-joined. Obrigatoriamente o último
    /// campo declarado no registro e do tipo <c>string?</c>.
    /// </summary>
    /// <remarks>
    /// Usado por registros cujo conteúdo é parametrizável em tempo de execução — por exemplo,
    /// o <c>RZ_CONT</c> do Registro I550 da ECD, cujas colunas são definidas dinamicamente pelo
    /// Registro I510. O <see cref="Gerador.EscritorSpedTxt"/> serializa o valor diretamente
    /// (sem pipes adicionais internos), pois o valor já inclui os separadores.
    /// </remarks>
    public bool CapturaTudo { get; init; }

    /// <summary>
    /// Quando <c>true</c>, marca este campo como o <b>campo-arquivo</b> de um registro multi-linha
    /// (ver <see cref="RegistroSpedAttribute.TokenFimArquivo"/>) — por exemplo o <c>ARQ_RTF</c> de
    /// J800/J801, que carrega um arquivo RTF de até 30 MB com quebras de linha CRLF internas. O
    /// leitor captura tudo entre o separador anterior e o último <c>|</c> do registro montado
    /// (o campo seguinte é o token de fim), preservando os <c>|</c> e CRLFs embutidos. Deve ser do
    /// tipo <c>string?</c> e ser seguido por exatamente um campo (o terminador <c>IND_FIM_*</c>).
    /// Difere de <see cref="CapturaTudo"/>: este não é o último campo e delimita pelo terminador,
    /// não pelo fim da linha.
    /// </summary>
    public bool CampoArquivo { get; init; }
}
