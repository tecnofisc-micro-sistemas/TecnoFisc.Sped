using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C165 — Complemento — Operações com Combustíveis (cód. 01).
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 75-76.
/// </summary>
[RegistroSped(Codigo = "C165", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC165 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C165";

    /// <summary>Código do participante — transportador, se houver (campo 02 do Registro 0150).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Placa de identificação do veículo.</summary>
    [CampoSped(Ordem = 3, Tamanho = 7)]
    public string? VeicId { get; set; }

    /// <summary>Código da autorização fornecido pela SEFAZ (combustíveis).</summary>
    [CampoSped(Ordem = 4, Tamanho = 0)]
    public string? CodAut { get; set; }

    /// <summary>Número do Passe Fiscal.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? NrPasse { get; set; }

    /// <summary>Hora da saída das mercadorias, no padrão "hhmmss".</summary>
    [CampoSped(Ordem = 6, Tamanho = 6)]
    public string? Hora { get; set; }

    /// <summary>Temperatura em graus Celsius utilizada para quantificação do volume de combustível.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 1)]
    public decimal? Temper { get; set; }

    /// <summary>Quantidade de volumes transportados.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0)]
    public int? QtdVol { get; set; }

    /// <summary>Peso bruto dos volumes transportados (em kg).</summary>
    [CampoSped(Ordem = 9, Tamanho = 0, Decimais = 2)]
    public decimal? PesoBrt { get; set; }

    /// <summary>Peso líquido dos volumes transportados (em kg).</summary>
    [CampoSped(Ordem = 10, Tamanho = 0, Decimais = 2)]
    public decimal? PesoLiq { get; set; }

    /// <summary>Nome do motorista.</summary>
    [CampoSped(Ordem = 11, Tamanho = 60)]
    public string? NomMot { get; set; }

    /// <summary>CPF do motorista (11 dígitos, sem máscara).</summary>
    [CampoSped(Ordem = 12, Tamanho = 11)]
    public Cpf? Cpf { get; set; }

    /// <summary>Sigla da UF da placa do veículo.</summary>
    [CampoSped(Ordem = 13, Tamanho = 2)]
    public string? UfId { get; set; }
}
