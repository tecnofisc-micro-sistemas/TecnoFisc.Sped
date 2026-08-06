using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

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

    public override int VersaoLeiaute
        => int.TryParse(CodVer, out int versao) ? versao : 0;

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

public enum SituacaoSintetica
{
    Ativa = 1,
    Inativa = 2,
}

[Flags]
public enum PermissoesSinteticas
{
    Leitura = 1,
    Escrita = 2,
}

[RegistroSped(Codigo = "E100", Nivel = 1, Bloco = "E")]
public sealed class RegistroE100Sintetico : RegistroSped
{
    public override string Codigo => "E100";

    [CampoSped(Ordem = 2, Tamanho = 2)]
    public SituacaoSintetica? Situacao { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 2)]
    public PermissoesSinteticas? Permissoes { get; set; }
}

public enum EnumByteSintetico : byte
{
    Maximo = byte.MaxValue,
}

public enum EnumSByteSintetico : sbyte
{
    Minimo = sbyte.MinValue,
}

public enum EnumInt16Sintetico : short
{
    Um = 1,
}

public enum EnumUInt16Sintetico : ushort
{
    Maximo = ushort.MaxValue,
}

public enum EnumInt32Sintetico : int
{
    Minimo = int.MinValue,
}

public enum EnumUInt32Sintetico : uint
{
    Maximo = uint.MaxValue,
}

public enum EnumInt64Sintetico : long
{
    Minimo = long.MinValue,
}

public enum EnumUInt64Sintetico : ulong
{
    Maximo = ulong.MaxValue,
}

[RegistroSped(Codigo = "E200", Nivel = 1, Bloco = "E")]
public sealed class RegistroE200Sintetico : RegistroSped
{
    public override string Codigo => "E200";

    [CampoSped(Ordem = 2)]
    public EnumByteSintetico? ValorByte { get; set; }

    [CampoSped(Ordem = 3)]
    public EnumSByteSintetico? ValorSByte { get; set; }

    [CampoSped(Ordem = 4)]
    public EnumInt16Sintetico? ValorInt16 { get; set; }

    [CampoSped(Ordem = 5)]
    public EnumUInt16Sintetico? ValorUInt16 { get; set; }

    [CampoSped(Ordem = 6)]
    public EnumInt32Sintetico? ValorInt32 { get; set; }

    [CampoSped(Ordem = 7)]
    public EnumUInt32Sintetico? ValorUInt32 { get; set; }

    [CampoSped(Ordem = 8)]
    public EnumInt64Sintetico? ValorInt64 { get; set; }

    [CampoSped(Ordem = 9)]
    public EnumUInt64Sintetico? ValorUInt64 { get; set; }
}

public enum IndicadorTextualSintetico
{
    [SpedValor("N")]
    Nao,

    [SpedValor("S")]
    Sim,
}

[RegistroSped(Codigo = "E300", Nivel = 1, Bloco = "E")]
public sealed class RegistroE300Sintetico : RegistroSped
{
    public override string Codigo => "E300";

    [CampoSped(Ordem = 2)]
    public IndicadorTextualSintetico? Indicador { get; set; }
}

/// <summary>
/// Registro sintético com campo-arquivo embutido — espelha a forma de J800/J801 da ECD
/// (TIPO_DOC, DESC_RTF, HASH_RTF, ARQ_RTF, IND_FIM_RTF). <c>ArqRtf</c> é o campo-arquivo
/// (<see cref="CampoSpedAttribute.CampoArquivo"/>) e o registro é multi-linha via
/// <see cref="RegistroSpedAttribute.TokenFimArquivo"/> = <c>"Y800FIM"</c>. Exercita a leitura
/// de registros cujo conteúdo ocupa várias linhas físicas (CRLF embutido no arquivo).
/// </summary>
[RegistroSped(Codigo = "Y800", Nivel = 3, Bloco = "Y", TokenFimArquivo = "Y800FIM")]
public sealed class RegistroY800Sintetico : RegistroSped
{
    public override string Codigo => "Y800";

    [CampoSped(Ordem = 2)]
    public string? TipoDoc { get; set; }

    [CampoSped(Ordem = 3)]
    public string? DescRtf { get; set; }

    [CampoSped(Ordem = 4)]
    public string? HashRtf { get; set; }

    [CampoSped(Ordem = 5, CampoArquivo = true)]
    public string? ArqRtf { get; set; }

    [CampoSped(Ordem = 6)]
    public string? IndFimRtf { get; set; }
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

/// <summary>Registro que prova alias normativo distinto do membro CLR.</summary>
[RegistroSped(Codigo = "A100", Nivel = 1, Bloco = "A")]
public sealed class RegistroA100AliasSintetico : RegistroSped
{
    public override string Codigo => "A100";

    [CampoSped(Ordem = 2, Nome = "CODIGO")]
    public int CampoCodigo { get; set; }
}

/// <summary>
/// Registro sintético versionado: introduzido na versão 310 do leiaute, com campo que só
/// passa a existir a partir da versão 312. Exercita <c>IntroduzidoEm</c> no atributo do
/// registro e <c>DesdeVersao</c> no atributo de campo.
/// </summary>
[RegistroSped(Codigo = "Z100", Nivel = 2, Bloco = "Z", IntroduzidoEm = 310)]
public sealed class RegistroZ100Sintetico : RegistroSped
{
    public override string Codigo => "Z100";

    [CampoSped(Ordem = 2)]
    public string? CodConf { get; set; }

    [CampoSped(Ordem = 3, DesdeVersao = 312)]
    public string? IndComplemento { get; set; }
}

[RegistroSped(Codigo = "Z200", Nivel = 2, Bloco = "Z", IntroduzidoEm = 310)]
public sealed class RegistroZ200Sintetico : RegistroSped
{
    public override string Codigo => "Z200";

    [CampoSped(Ordem = 2)]
    public string? Antes { get; set; }

    [CampoSped(Ordem = 3, DesdeVersao = 312)]
    public string? Futuro { get; set; }

    [CampoSped(Ordem = 4)]
    public string? Depois { get; set; }
}
