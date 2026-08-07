from ecf_layout.field_names import apply, contar_atributos, contar_campos_casados


def test_adiciona_alias_quando_nao_existe():
    fonte = (
        "    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]\n"
        "    public decimal SdIniLal { get; set; }\n"
    )
    assert 'Nome = "SD_INI_LAL"' in apply(fonte, {4: "SD_INI_LAL"})


def test_substitui_alias_existente():
    fonte = (
        '    [CampoSped(Ordem = 5, Tamanho = 1, Nome = "ERRADO")]\n'
        "    public int Campo { get; set; }\n"
    )
    resultado = apply(fonte, {5: "IND_SD_INI_LAL"})
    assert 'Nome = "IND_SD_INI_LAL"' in resultado
    assert "ERRADO" not in resultado


def test_preserva_campo_ausente_do_manifesto():
    fonte = "    [CampoSped(Ordem = 9)]\n    public int Campo { get; set; }\n"
    assert apply(fonte, {}) == fonte


def test_substitui_alias_quando_nome_e_o_primeiro_argumento():
    """Nome como primeiro argumento sobra vírgula solta se _NOME.sub só remover o
    texto do Nome sem reconstruir a lista de argumentos (achado do code review):
    o resultado ingênuo era `[CampoSped(, Ordem = 6)]`, que não compila."""
    fonte = (
        '    [CampoSped(Nome = "ERRADO", Ordem = 6, Tamanho = 1)]\n'
        "    public string? Campo { get; set; }\n"
    )
    resultado = apply(fonte, {6: "CERTO"})
    assert '[CampoSped(Ordem = 6, Tamanho = 1, Nome = "CERTO")]' in resultado
    assert "ERRADO" not in resultado
    assert ",  Ordem" not in resultado
    assert "(, " not in resultado
    assert "(," not in resultado


def test_contar_atributos_conta_todo_campo_sped_independente_de_casar():
    fonte = (
        "    [CampoSped(Ordem = 2, Tamanho = 1)]\n"
        "    public int Primeiro { get; set; }\n"
        "\n"
        "    [CampoSped(Ordem = 3, Tamanho = 1)]\n"
        "    [Obsolete]\n"
        "    public int Segundo { get; set; }\n"
    )
    assert contar_atributos(fonte) == 2


def test_contar_campos_casados_ignora_atributo_com_comentario_ou_segundo_atributo_antes_de_public():
    """`_CAMPO` exige `public` imediatamente após `]`; um segundo atributo (ex.:
    `[Obsolete]`) ou comentário intercalado entre `[CampoSped(...)]` e `public`
    faz `apply` ignorar o campo em silêncio. `contar_campos_casados` expõe essa
    divergência para que quem orquestra a reescrita (o subcomando `field-names`)
    possa falhar alto em vez de aplicar o alias parcialmente sem avisar."""
    fonte = (
        "    [CampoSped(Ordem = 2, Tamanho = 1)]\n"
        "    public int Primeiro { get; set; }\n"
        "\n"
        "    [CampoSped(Ordem = 3, Tamanho = 1)]\n"
        "    [Obsolete]\n"
        "    public int Segundo { get; set; }\n"
    )
    assert contar_atributos(fonte) == 2
    assert contar_campos_casados(fonte) == 1

    resultado = apply(fonte, {2: "PRIMEIRO", 3: "SEGUNDO"})
    assert 'Nome = "PRIMEIRO"' in resultado
    assert 'Nome = "SEGUNDO"' not in resultado
