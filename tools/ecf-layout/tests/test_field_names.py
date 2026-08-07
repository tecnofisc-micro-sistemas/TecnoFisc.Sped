from ecf_layout.field_names import aplicar


def test_adiciona_alias_quando_nao_existe():
    fonte = (
        "    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]\n"
        "    public decimal SdIniLal { get; set; }\n"
    )
    assert 'Nome = "SD_INI_LAL"' in aplicar(fonte, {4: "SD_INI_LAL"})


def test_substitui_alias_existente():
    fonte = (
        '    [CampoSped(Ordem = 5, Tamanho = 1, Nome = "ERRADO")]\n'
        "    public int Campo { get; set; }\n"
    )
    resultado = aplicar(fonte, {5: "IND_SD_INI_LAL"})
    assert 'Nome = "IND_SD_INI_LAL"' in resultado
    assert "ERRADO" not in resultado


def test_preserva_campo_ausente_do_manifesto():
    fonte = "    [CampoSped(Ordem = 9)]\n    public int Campo { get; set; }\n"
    assert aplicar(fonte, {}) == fonte
