using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1320 - Volume de Vendas. Nivel hierarquico 4, ocorrencia varios por Registro 1310.
/// Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 277-278.
/// </summary>
[RegistroSped(Codigo = "1320", Nivel = 4, Bloco = "1")]
public sealed partial class Registro1320 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1320";

    /// <summary>Numero do bico associado ao tanque do registro pai.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public int NumBico { get; set; }

    /// <summary>Numero da intervencao atribuido pelo orgao competente ou pelo declarante.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public int? NrInterv { get; set; }

    /// <summary>Motivo da intervencao.</summary>
    [CampoSped(Ordem = 4, Tamanho = 50)]
    public string? MotInterv { get; set; }

    /// <summary>Nome do tecnico autorizado responsavel pela intervencao.</summary>
    [CampoSped(Ordem = 5, Tamanho = 30)]
    public string? NomInterv { get; set; }

    /// <summary>CNPJ da empresa responsavel pela intervencao.</summary>
    [CampoSped(Ordem = 6, Tamanho = 14)]
    public Cnpj? CnpjInterv { get; set; }

    /// <summary>CPF do tecnico responsavel pela intervencao.</summary>
    [CampoSped(Ordem = 7, Tamanho = 11)]
    public Cpf? CpfInterv { get; set; }

    /// <summary>Valor da leitura final do contador no fechamento do bico.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal ValFecha { get; set; }

    /// <summary>Valor da leitura inicial do contador na abertura do bico.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal ValAbert { get; set; }

    /// <summary>Volume em litros relativo as afericoes efetuadas.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 3)]
    public decimal? VolAferi { get; set; }

    /// <summary>Volume de vendas do bico, em litros.</summary>
    [CampoSped(Ordem = 11, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal VolVendas { get; set; }
}
