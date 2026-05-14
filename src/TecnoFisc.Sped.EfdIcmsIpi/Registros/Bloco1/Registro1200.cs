using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1200 - Controle de Creditos Fiscais - ICMS. Nivel hierarquico 2,
/// ocorrencia varios por arquivo. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 272-273.
/// </summary>
[RegistroSped(Codigo = "1200", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1200 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1200";

    /// <summary>Codigo de ajuste, conforme Tabela 5.1.1.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Obrigatorio = true)]
    public string? CodAjApur { get; set; }

    /// <summary>Saldo de creditos fiscais de periodos anteriores.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal SldCred { get; set; }

    /// <summary>Total de credito apropriado no mes.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal CredApr { get; set; }

    /// <summary>Total de creditos recebidos por transferencia.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal CredReceb { get; set; }

    /// <summary>Total de creditos utilizados no periodo.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal CredUtil { get; set; }

    /// <summary>Saldo de credito fiscal acumulado a transportar para o periodo seguinte.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal SldCredFim { get; set; }
}
