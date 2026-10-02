namespace TecnoFisc.Sped.Core.ValueObjects;

/// <summary>
/// Chave de Acesso de DF-e (NF-e, NFC-e, CT-e). 44 caracteres com DV módulo 11 (ASCII menos 48).
/// Estrutura: cUF(2) AAMM(4) CNPJ(14) mod(2) serie(3) nNF(9) tpEmis(1) cNF(8) cDV(1).
/// </summary>
public readonly struct ChaveAcesso : IEquatable<ChaveAcesso>
{
    public const int Tamanho = 44;

    private readonly string? _valor;

    private ChaveAcesso(string valor) => _valor = valor;

    /// <exception cref="FormatException">Quando o valor não é uma chave de acesso válida.</exception>
    public static ChaveAcesso Create(ReadOnlySpan<char> valor)
    {
        if (!TentarCriar(valor, out var chave))
            throw new FormatException($"Valor não é uma chave de acesso válida: '{valor}'.");
        return chave;
    }

    public static bool TentarCriar(ReadOnlySpan<char> valor, out ChaveAcesso chave)
    {
        Span<char> digitos = stackalloc char[Tamanho];
        if (!IdentificadorAlfanumericoHelper.TentarExtrair(valor, digitos, 6, 12))
        {
            chave = default;
            return false;
        }

        if (!ValidarDv(digitos))
        {
            chave = default;
            return false;
        }

        // NT 2023.002: emitente pessoa física usa CPF preenchido à esquerda até 14 posições.
        var documento = digitos.Slice(6, 14);
        if (!Cnpj.TentarCriar(documento, out _)
            && !(PermiteCpf(digitos) && documento[..3].SequenceEqual("000") && Cpf.TentarCriar(documento[3..], out _)))
        {
            chave = default;
            return false;
        }

        // cUF deve ser um código IBGE de UF válido.
        int cuf = (digitos[0] - '0') * 10 + (digitos[1] - '0');
        if (!CodigosUf.IsValid(cuf))
        {
            chave = default;
            return false;
        }

        chave = new ChaveAcesso(new string(digitos));
        return true;
    }

    private static bool ValidarDv(ReadOnlySpan<char> digitos)
    {
        int soma = 0;
        int peso = 2;
        // Pesos 2..9 ciclando da direita para a esquerda nos 43 primeiros dígitos.
        for (int i = 42; i >= 0; i--)
        {
            soma += (digitos[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }
        int resto = soma % 11;
        int dv = 11 - resto;
        if (dv >= 10) dv = 0;
        return dv == digitos[43] - '0';
    }

    // NT 2023.002, C02a-10: evita interpretar CNPJ inválido como CPF em uma série comum.
    private static bool PermiteCpf(ReadOnlySpan<char> chave)
    {
        int modelo = (chave[20] - '0') * 10 + chave[21] - '0';
        if (modelo is not (55 or 65)) return false;
        int serie = (chave[22] - '0') * 100 + (chave[23] - '0') * 10 + chave[24] - '0';
        // NT 2021.002: na NFF o quinto dígito de nNF identifica CPF (2).
        if (chave[34] == '3') return chave[29] == '2';
        return serie is >= 890 and <= 899 or >= 910 and <= 969;
    }

    public ReadOnlySpan<char> AsSpan() => _valor.AsSpan();

    /// <summary>Código IBGE da UF do emitente (posições 0-1).</summary>
    public int CodigoUf => _valor is null ? 0 : (_valor[0] - '0') * 10 + (_valor[1] - '0');

    /// <summary>Ano da emissão no formato AA (posições 2-3).</summary>
    public int Ano => _valor is null ? 0 : (_valor[2] - '0') * 10 + (_valor[3] - '0');

    /// <summary>Mês da emissão (posições 4-5).</summary>
    public int Mes => _valor is null ? 0 : (_valor[4] - '0') * 10 + (_valor[5] - '0');

    /// <summary>CNPJ do emitente (posições 6-19). Para pessoa física, consulte <see cref="CpfEmitente"/>.</summary>
    /// <exception cref="FormatException">A chave contém CPF em vez de CNPJ.</exception>
    public Cnpj CnpjEmitente => _valor is null ? default : Cnpj.Create(_valor.AsSpan(6, 14));

    /// <summary>CPF do emitente quando a chave utiliza CPF com zeros à esquerda; null para CNPJ.</summary>
    public Cpf? CpfEmitente => _valor is not null && _valor.AsSpan(6, 3).SequenceEqual("000")
        && !Cnpj.TentarCriar(_valor.AsSpan(6, 14), out _)
        && Cpf.TentarCriar(_valor.AsSpan(9, 11), out var cpf) ? cpf : null;

    /// <summary>Modelo do documento fiscal: 55 = NF-e, 65 = NFC-e, 57 = CT-e (posições 20-21).</summary>
    public int Modelo => _valor is null ? 0 : (_valor[20] - '0') * 10 + (_valor[21] - '0');

    /// <summary>Série do documento (posições 22-24).</summary>
    public int Serie => _valor is null ? 0 : InteiroDePosicao(_valor, 22, 3);

    /// <summary>Número do documento (posições 25-33).</summary>
    public int Numero => _valor is null ? 0 : InteiroDePosicao(_valor, 25, 9);

    /// <summary>Forma de emissão (posição 34): 1 normal, 2 contingência FS, 3 SCAN, etc.</summary>
    public int TipoEmissao => _valor is null ? 0 : _valor[34] - '0';

    /// <summary>Código numérico aleatório de unicidade (posições 35-42).</summary>
    public string CodigoNumerico => _valor is null ? new string('0', 8) : _valor.Substring(35, 8);

    /// <summary>Dígito verificador (posição 43).</summary>
    public int DigitoVerificador => _valor is null ? 0 : _valor[43] - '0';

    private static int InteiroDePosicao(string valor, int inicio, int tamanho)
    {
        int n = 0;
        for (int i = 0; i < tamanho; i++)
            n = n * 10 + (valor[inicio + i] - '0');
        return n;
    }

    public override string ToString() => _valor ?? new string('0', Tamanho);

    public bool Equals(ChaveAcesso other) => string.Equals(_valor, other._valor, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is ChaveAcesso c && Equals(c);
    public override int GetHashCode() => _valor?.GetHashCode(StringComparison.Ordinal) ?? 0;
    public static bool operator ==(ChaveAcesso left, ChaveAcesso right) => left.Equals(right);
    public static bool operator !=(ChaveAcesso left, ChaveAcesso right) => !left.Equals(right);

    public static implicit operator string(ChaveAcesso chave) => chave.ToString();
}
