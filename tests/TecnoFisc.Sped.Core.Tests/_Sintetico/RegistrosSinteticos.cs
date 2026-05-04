using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Core.Tests._Sintetico;

/// <summary>
/// Mini "layout" fictício usado pelos testes de catálogo, leitor e pilha hierárquica.
/// Modelado para se parecer com a EFD Contribuições (registros 0000 / C001 / C100 / C170 / 9999),
/// o que exercita os principais cenários: tipos primitivos, value objects fiscais, datas,
/// campos opcionais e a hierarquia Pai/Filhos.
/// </summary>
[RegistroSped(Codigo = "0000", Nivel = 0, Bloco = "0")]
public sealed class Registro0000Sintetico : RegistroSped
{
    public override string Codigo => "0000";

    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public string? CodVer { get; set; }

    [CampoSped(Ordem = 3, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    [CampoSped(Ordem = 4, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 100)]
    public string? Nome { get; set; }

    [CampoSped(Ordem = 6)]
    public Cnpj Cnpj { get; set; }
}

[RegistroSped(Codigo = "C001", Nivel = 1, Bloco = "C")]
public sealed class RegistroC001Sintetico : RegistroSped
{
    public override string Codigo => "C001";

    [CampoSped(Ordem = 2)]
    public int IndMov { get; set; }
}

[RegistroSped(Codigo = "C100", Nivel = 2, Bloco = "C")]
public sealed class RegistroC100Sintetico : RegistroSped
{
    public override string Codigo => "C100";

    [CampoSped(Ordem = 2)]
    public string? IndOper { get; set; }

    [CampoSped(Ordem = 3)]
    public int CodPart { get; set; }

    [CampoSped(Ordem = 4, Decimais = 2)]
    public decimal VlDoc { get; set; }

    [CampoSped(Ordem = 5)]
    public Cfop Cfop { get; set; }
}

[RegistroSped(Codigo = "C170", Nivel = 3, Bloco = "C")]
public sealed class RegistroC170Sintetico : RegistroSped
{
    public override string Codigo => "C170";

    [CampoSped(Ordem = 2)]
    public int NumItem { get; set; }

    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    [CampoSped(Ordem = 4)]
    public int Quantidade { get; set; }

    [CampoSped(Ordem = 5, Decimais = 2)]
    public decimal VlItem { get; set; }
}

[RegistroSped(Codigo = "0990", Nivel = 1, Bloco = "0")]
public sealed class Registro0990Sintetico : RegistroSped
{
    public override string Codigo => "0990";

    [CampoSped(Ordem = 2)]
    public int QtdLin0 { get; set; }
}

[RegistroSped(Codigo = "C990", Nivel = 1, Bloco = "C")]
public sealed class RegistroC990Sintetico : RegistroSped
{
    public override string Codigo => "C990";

    [CampoSped(Ordem = 2)]
    public int QtdLinC { get; set; }
}

[RegistroSped(Codigo = "9999", Nivel = 0, Bloco = "9")]
public sealed class Registro9999Sintetico : RegistroSped
{
    public override string Codigo => "9999";

    [CampoSped(Ordem = 2)]
    public int QtdLin { get; set; }
}
