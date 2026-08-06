using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.Ecf.Tests.Manifesto;

public sealed class AssertRegistroEcfCompatibilityTests
{
    [Theory]
    [InlineData("E020", "DT_AP_LAL", false)]
    [InlineData("E020", "DT_LIM_LAL", false)]
    [InlineData("M010", "DT_AP_LAL", true)]
    [InlineData("M010", "DT_LIM_LAL", false)]
    [InlineData("Y620", "DATA_AQUIS", true)]
    [InlineData("X280", "VIG_INI", true)]
    [InlineData("X280", "VIG_FIM", true)]
    public void CampoDataDeOitoPosicoes_ExigeDateOnly(
        string codigo,
        string campo,
        bool obrigatorio)
    {
        var dateOnly = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            codigo,
            campo,
            typeof(DateOnly),
            tamanho: 8,
            decimais: 0,
            obrigatorio);
        var stringLexical = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            codigo,
            campo,
            typeof(string),
            tamanho: 8,
            decimais: 0,
            obrigatorio);

        dateOnly.Should().NotThrow();
        stringLexical.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage($"*{codigo}*{campo}*tipo*incompatível*String*");
    }

    [Theory]
    [InlineData("VIG_INI", "C")]
    [InlineData("VIG_FIM", "N")]
    public void CampoVigenciaDeOitoPosicoes_IndependeDoTipoLexical(
        string nome,
        string tipoManifesto)
    {
        ManifestoCampoEcf campo = CriarCampo(
            nome,
            descricao: "Prazo normativo.",
            tipoManifesto,
            tamanho: "8");

        AssertRegistroEcf.TipoCompativel(campo, typeof(DateOnly)).Should().BeTrue();
        AssertRegistroEcf.TipoCompativel(campo, typeof(string)).Should().BeFalse();
    }

    [Theory]
    [InlineData("DATA", "C")]
    [InlineData("DT_REFERENCIA", "N")]
    [InlineData("PREFIXO_DATA_SUFIXO", "D")]
    [InlineData("REFERENCIA_DATA", "C")]
    [InlineData("VIG_INI", "N")]
    public void SegmentoDataDelimitadoPorUnderscore_ClassificaDateOnly(
        string nome,
        string tipoManifesto)
    {
        ManifestoCampoEcf campo = CriarCampo(
            nome,
            descricao: "Campo normativo.",
            tipoManifesto,
            tamanho: "8");

        AssertRegistroEcf.TipoCompativel(campo, typeof(DateOnly)).Should().BeTrue();
        AssertRegistroEcf.TipoCompativel(campo, typeof(string)).Should().BeFalse();
    }

    [Theory]
    [InlineData("VIGÊNCIA")]
    [InlineData("VIG.ENCIA")]
    [InlineData("DATA-REFERENCIA")]
    [InlineData("X/VIG/Y")]
    [InlineData("DT REFERENCIA")]
    [InlineData("X DATA Y")]
    [InlineData("DATA.REFERENCIA")]
    public void TokenDataSemDelimitadorUnderscore_PermaneceGenerico(string nome)
    {
        ManifestoCampoEcf campo = CriarCampo(
            nome,
            descricao: "Identificador genérico.",
            tipoManifesto: "C",
            tamanho: "8");

        AssertRegistroEcf.TipoCompativel(campo, typeof(string)).Should().BeTrue();
        AssertRegistroEcf.TipoCompativel(campo, typeof(DateOnly)).Should().BeFalse();
    }

    [Theory]
    [InlineData("IDENTIFICADOR", "Data informada pelo contribuinte.", "C", "8")]
    [InlineData("DATA_AQUIS", "Identificador genérico.", "C", "7")]
    [InlineData("DT_REFERENCIA", "Identificador genérico.", "N", "9")]
    [InlineData("VIGENCIA", "Identificador genérico.", "C", "8")]
    public void CampoProximoDeData_SemTokenEFormaNormativa_PermaneceGenerico(
        string nome,
        string descricao,
        string tipoManifesto,
        string tamanho)
    {
        ManifestoCampoEcf campo = CriarCampo(nome, descricao, tipoManifesto, tamanho);

        AssertRegistroEcf.TipoCompativel(campo, typeof(string)).Should().BeTrue();
        AssertRegistroEcf.TipoCompativel(campo, typeof(DateOnly)).Should().BeFalse();
    }

    [Fact]
    public void TinSubstitutaComNotinExteriorEBrasil_AceitaString()
    {
        var act = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            "W100",
            "TIN_SUBSTITUTA",
            typeof(string),
            tamanho: 14,
            decimais: 0,
            obrigatorio: false);

        act.Should().NotThrow();
    }

    [Fact]
    public void DestinatarioComDocumentoDeOnzeOuQuatorzePosicoes_AceitaString()
    {
        var act = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            "Y730",
            "DESTINATARIO",
            typeof(string),
            tamanho: 14,
            decimais: 0,
            obrigatorio: true);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("Se residente no Brasil, informar o CNPJ; no exterior, usar NOTIN.", "N")]
    [InlineData("Identificador fiscal; quando aplicável, pode ser CNPJ.", "C")]
    [InlineData("Documento CNPJ (14) ou CNP (11).", "C")]
    public void NomeNeutro_NaoInfereCnpjDaDescricao(string descricao, string tipoManifesto)
    {
        ManifestoCampoEcf campo = CriarCampo(
            "IDENTIFICADOR",
            descricao,
            tipoManifesto,
            tamanho: "14");

        AssertRegistroEcf.TipoCompativel(campo, typeof(string)).Should().BeTrue();
    }

    [Theory]
    [InlineData("0930", "IDENT_CPF_CNPJ", 14, true)]
    [InlineData("X357", "NIF/CNPJ", 0, true)]
    [InlineData("Y600", "CPF_CNPJ", 14, false)]
    public void CampoDocumentoComposto_AceitaStringSemForcarValueObject(
        string codigo,
        string campo,
        int tamanho,
        bool obrigatorio)
    {
        var act = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            codigo,
            campo,
            typeof(string),
            tamanho,
            decimais: 0,
            obrigatorio);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("Y600", "CPF_REP_LEG", false)]
    [InlineData("Y612", "CPF", true)]
    public void CampoCpfNumerico_AceitaCpf(
        string codigo,
        string campo,
        bool obrigatorio)
    {
        var act = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            codigo,
            campo,
            typeof(Cpf),
            tamanho: 11,
            decimais: 0,
            obrigatorio);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("Q100", "DATA")]
    [InlineData("Y730", "DATA")]
    public void CampoDataNumerico_AceitaDateOnly(string codigo, string campo)
    {
        var act = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            codigo,
            campo,
            typeof(DateOnly),
            tamanho: 8,
            decimais: 0,
            obrigatorio: true);

        act.Should().NotThrow();
    }

    [Fact]
    public void CampoCpfExclusivo_RejeitaCnpj()
    {
        var act = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            "Y612",
            "CPF",
            typeof(Cnpj),
            tamanho: 11,
            decimais: 0,
            obrigatorio: true);

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*Y612*CPF*tipo*incompatível*Cnpj*");
    }

    [Fact]
    public void CampoCnpjExclusivo_PreservaValueObjectForte()
    {
        var correto = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            "0000",
            "CNPJ",
            typeof(Cnpj),
            tamanho: 14,
            decimais: 0,
            obrigatorio: true);
        var stringGenerica = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            "0000",
            "CNPJ",
            typeof(string),
            tamanho: 14,
            decimais: 0,
            obrigatorio: true);

        correto.Should().NotThrow();
        stringGenerica.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*0000*CNPJ*tipo*incompatível*String*");
    }

    [Theory]
    [InlineData("X357", "NIF/CNPJ", 0, true)]
    [InlineData("Y600", "CPF_CNPJ", 14, false)]
    public void CampoDocumentoComposto_RejeitaValueObjectDeDocumentoUnico(
        string codigo,
        string campo,
        int tamanho,
        bool obrigatorio)
    {
        var act = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            codigo,
            campo,
            typeof(Cnpj),
            tamanho,
            decimais: 0,
            obrigatorio);

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage($"*{codigo}*{campo}*tipo*incompatível*Cnpj*");
    }

    [Fact]
    public void ChaveComNifECnpjEmCamposSeparados_PreservaAmbosComoString()
    {
        var nif = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            "X340",
            "NIF",
            typeof(string),
            tamanho: 0,
            decimais: 0,
            obrigatorio: true);
        var cnpj = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            "X340",
            "CNPJ",
            typeof(string),
            tamanho: 14,
            decimais: 0,
            obrigatorio: false);
        var nifForte = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            "X340",
            "NIF",
            typeof(Cnpj),
            tamanho: 0,
            decimais: 0,
            obrigatorio: true);
        var cnpjForte = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            "X340",
            "CNPJ",
            typeof(Cnpj),
            tamanho: 14,
            decimais: 0,
            obrigatorio: false);

        nif.Should().NotThrow();
        cnpj.Should().NotThrow();
        nifForte.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*X340*NIF*tipo*incompatível*Cnpj*");
        cnpjForte.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*X340*CNPJ*tipo*incompatível*Cnpj*");
    }

    [Fact]
    public void NifIrmaoNaoTransformaCnpjQualificadoEmDocumentoComposto()
    {
        ManifestoCampoEcf nif = CriarCampo(
            "NIF",
            descricao: "Identificador fiscal no exterior.",
            tipoManifesto: "C",
            tamanho: "-");
        ManifestoCampoEcf cnpjEstabelecimento = CriarCampo(
            "CNPJ_ESTABELECIMENTO",
            descricao: "CNPJ exclusivo do estabelecimento.",
            tipoManifesto: "C",
            tamanho: "14");
        ManifestoRegistroEcf registro = CriarRegistro(nif, cnpjEstabelecimento);

        AssertRegistroEcf.TipoCompativel(registro, cnpjEstabelecimento, typeof(Cnpj))
            .Should().BeTrue();
        AssertRegistroEcf.TipoCompativel(registro, cnpjEstabelecimento, typeof(string))
            .Should().BeFalse();
    }

    private static ManifestoCampoEcf CriarCampo(
        string nome,
        string descricao,
        string tipoManifesto,
        string tamanho)
        => new()
        {
            Number = 2,
            Name = nome,
            Description = descricao,
            Type = tipoManifesto,
            Size = tamanho,
            Decimals = "-",
            Required = "Não",
            ValidValues = "-",
        };

    private static ManifestoRegistroEcf CriarRegistro(params ManifestoCampoEcf[] campos)
        => new()
        {
            Code = "TESTE",
            Block = "T",
            Title = "Registro sintético",
            PageStart = 1,
            PageEnd = 2,
            Level = "1",
            Occurrence = "1:1",
            Reviewed = true,
            Fields = campos,
        };
}
