using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.Ecf.Tests.Manifesto;

public sealed class AssertRegistroEcfCompatibilityTests
{
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
        var campo = new ManifestoCampoEcf
        {
            Number = 2,
            Name = "IDENTIFICADOR",
            Description = descricao,
            Type = tipoManifesto,
            Size = "14",
            Decimals = "-",
            Required = "Não",
            ValidValues = "-",
        };

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

    [Fact]
    public void CampoDocumentoComposto_RejeitaValueObjectDeDocumentoUnico()
    {
        var act = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            "Y600",
            "CPF_CNPJ",
            typeof(Cnpj),
            tamanho: 14,
            decimais: 0,
            obrigatorio: false);

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*Y600*CPF_CNPJ*tipo*incompatível*Cnpj*");
    }
}
