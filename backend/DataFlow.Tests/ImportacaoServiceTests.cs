using System.Globalization;
using System.Threading;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging.Abstractions;
using DataFlow.Api.DTOs.Responses;
using DataFlow.Api.Services;

namespace DataFlow.Tests;

public class ImportacaoServiceTests
{
    private static readonly string[] CabecalhosOficiais =
    [
        "Matrícula*",
        "Nome*",
        "Curso*",
        "Data de Nascimento*",
        "Email",
        "Mensalidade"
    ];

    private readonly ImportacaoService _service = new(NullLogger<ImportacaoService>.Instance);

    [Fact]
    public void Validar_PlanilhaValidaCom10Registros_DeveRetornarSucesso()
    {
        using var stream = CriarWorkbookComRegistros(10);

        var resultado = _service.Validar(stream);

        Assert.True(resultado.Sucesso);
        Assert.All(resultado.Validacoes, v => Assert.True(v.Sucesso));
    }

    [Fact]
    public void Validar_PlanilhaValidaCom50Registros_DeveRetornarSucesso()
    {
        using var stream = CriarWorkbookComRegistros(50);

        var resultado = _service.Validar(stream);

        Assert.True(resultado.Sucesso);
        Assert.All(resultado.Validacoes, v => Assert.True(v.Sucesso));
    }

    [Fact]
    public void Validar_PlanilhaComApenas9Registros_DeveFalharNumeroMinimo()
    {
        using var stream = CriarWorkbookComRegistros(9);

        var resultado = _service.Validar(stream);

        Assert.False(resultado.Sucesso);
        var validacao = ObterValidacao(resultado, "numeroMinimoRegistros");
        Assert.False(validacao.Sucesso);
        Assert.Equal("A planilha possui 9 registros. O mínimo permitido é 10.", validacao.MensagemErro);
    }

    [Fact]
    public void Validar_ColunaFaltando_DeveFalharEstruturaEColunasObrigatorias()
    {
        using var stream = CriarWorkbookComCabecalhos(["Matrícula*", "Nome*", "Data de Nascimento*", "Email", "Mensalidade"], 10);

        var resultado = _service.Validar(stream);

        Assert.False(resultado.Sucesso);
        Assert.False(ObterValidacao(resultado, "estrutura").Sucesso);
        Assert.False(ObterValidacao(resultado, "colunasObrigatorias").Sucesso);
        Assert.Equal("A coluna obrigatória \"Curso*\" não foi encontrada.", ObterValidacao(resultado, "colunasObrigatorias").MensagemErro);
    }

    [Fact]
    public void Validar_ColunaExtra_DeveFalharEstrutura()
    {
        using var stream = CriarWorkbookComColunaExtra(10);

        var resultado = _service.Validar(stream);

        Assert.False(resultado.Sucesso);
        Assert.False(ObterValidacao(resultado, "estrutura").Sucesso);
        Assert.Equal("A estrutura da planilha não corresponde ao modelo padrão do sistema.", ObterValidacao(resultado, "estrutura").MensagemErro);
    }

    [Fact]
    public void Validar_ColunasForaDeOrdem_DeveFalharEstrutura()
    {
        using var stream = CriarWorkbookComCabecalhos(["Nome*", "Matrícula*", "Curso*", "Data de Nascimento*", "Email", "Mensalidade"], 10);

        var resultado = _service.Validar(stream);

        Assert.False(resultado.Sucesso);
        Assert.False(ObterValidacao(resultado, "estrutura").Sucesso);
    }

    [Fact]
    public void Validar_LinhasVaziasNaoContamComoRegistros_DeveContarApenasPreenchidas()
    {
        using var stream = CriarWorkbookComLinhasVaziasIntercaladas(10);

        var resultado = _service.Validar(stream);

        Assert.True(resultado.Sucesso);
        Assert.True(ObterValidacao(resultado, "numeroMinimoRegistros").Sucesso);
    }

    [Fact]
    public void Validar_CamposObrigatoriosVaziosNasLinhas_NaoFazemColunasObrigatoriasFalhar()
    {
        using var stream = CriarWorkbookComCamposVaziosNasLinhas(10);

        var resultado = _service.Validar(stream);

        Assert.True(ObterValidacao(resultado, "colunasObrigatorias").Sucesso);
    }

    [Fact]
    public void Validar_ArquivoXlsxInvalido_DeveTratarSemExceptionNaoControlada()
    {
        using var stream = new MemoryStream([0x00, 0x01, 0x02, 0x03, 0x04, 0x05]);

        var resultado = _service.Validar(stream);

        Assert.False(resultado.Sucesso);
        var validacao = ObterValidacao(resultado, "estrutura");
        Assert.False(validacao.Sucesso);
        Assert.Equal("Não foi possível ler a estrutura da planilha.", validacao.MensagemErro);
    }

    [Fact]
    public void Validar_RegistroCompletamenteValido_DeveRetornarStatusValido()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: 500.00m);

        var resultado = _service.Validar(stream);

        Assert.True(resultado.Sucesso);
        var registro = resultado.Registros[0];
        Assert.Equal("valido", registro.Status);
        Assert.Empty(registro.Erros);
        Assert.Equal("12345678", registro.Matricula);
        Assert.Equal("Aluno Teste", registro.Nome);
        Assert.Equal("Curso A", registro.Curso);
        Assert.Equal("01/01/2000", registro.DataNascimento);
        Assert.Equal("aluno@email.com", registro.Email);
        Assert.Equal("500", registro.Mensalidade);
    }

    [Fact]
    public void Validar_MatriculaVazia_DeveRetornarErro()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: null,
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: 500.00m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("erro", registro.Status);
        var erro = Assert.Single(registro.Erros);
        Assert.Equal("Matrícula", erro.Campo);
        Assert.Equal("Matrícula não preenchida.", erro.Mensagem);
    }

    [Fact]
    public void Validar_MatriculaCom7Digitos_DeveRetornarErro()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "1234567",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: 500.00m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("erro", registro.Status);
        var erro = Assert.Single(registro.Erros);
        Assert.Equal("Matrícula", erro.Campo);
        Assert.Equal("A matrícula deve conter somente números e possuir no mínimo 8 dígitos.", erro.Mensagem);
    }

    [Fact]
    public void Validar_MatriculaContendoLetra_DeveRetornarErro()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "1234567A",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: 500.00m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("erro", registro.Status);
        var erro = Assert.Single(registro.Erros);
        Assert.Equal("Matrícula", erro.Campo);
        Assert.Equal("A matrícula deve conter somente números e possuir no mínimo 8 dígitos.", erro.Mensagem);
    }

    [Fact]
    public void Validar_MatriculaComExatamente8Digitos_DeveRetornarValido()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "00123456",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: 500.00m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("valido", registro.Status);
        Assert.Empty(registro.Erros);
        Assert.Equal("00123456", registro.Matricula);
    }

    [Fact]
    public void Validar_NomeVazio_DeveRetornarErro()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: 500.00m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("erro", registro.Status);
        var erro = Assert.Single(registro.Erros);
        Assert.Equal("Nome", erro.Campo);
        Assert.Equal("Nome não preenchido.", erro.Mensagem);
    }

    [Fact]
    public void Validar_CursoVazio_DeveRetornarErro()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "Aluno Teste",
            curso: "",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: 500.00m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("erro", registro.Status);
        var erro = Assert.Single(registro.Erros);
        Assert.Equal("Curso", erro.Campo);
        Assert.Equal("Curso não preenchido.", erro.Mensagem);
    }

    [Fact]
    public void Validar_DataNascimentoVazia_DeveRetornarErro()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: null,
            email: "aluno@email.com",
            mensalidade: 500.00m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("erro", registro.Status);
        var erro = Assert.Single(registro.Erros);
        Assert.Equal("Data de Nascimento", erro.Campo);
        Assert.Equal("Data de nascimento não preenchida.", erro.Mensagem);
    }

    [Fact]
    public void Validar_DataNascimentoInvalida_DeveRetornarErro()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: "texto-invalido",
            email: "aluno@email.com",
            mensalidade: 500.00m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("erro", registro.Status);
        var erro = Assert.Single(registro.Erros);
        Assert.Equal("Data de Nascimento", erro.Campo);
        Assert.Equal("Data de nascimento inválida.", erro.Mensagem);
    }

    [Fact]
    public void Validar_AlunoComExatamente18Anos_DeveRetornarValido()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: DateTime.Today.AddYears(-18),
            email: "aluno@email.com",
            mensalidade: 500.00m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("valido", registro.Status);
        Assert.Empty(registro.Erros);
    }

    [Fact]
    public void Validar_AlunoComMenosDe18Anos_DeveRetornarErro()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: DateTime.Today.AddYears(-17),
            email: "aluno@email.com",
            mensalidade: 500.00m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("erro", registro.Status);
        var erro = Assert.Single(registro.Erros);
        Assert.Equal("Data de Nascimento", erro.Campo);
        Assert.Equal("O aluno deve possuir 18 anos completos ou mais.", erro.Mensagem);
    }

    [Fact]
    public void Validar_EmailVazio_DeveContinuarValido()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: null,
            mensalidade: 500.00m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("valido", registro.Status);
        Assert.Empty(registro.Erros);
        Assert.Null(registro.Email);
    }

    [Fact]
    public void Validar_MensalidadeVazia_DeveContinuarValido()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: null);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("valido", registro.Status);
        Assert.Empty(registro.Erros);
        Assert.Null(registro.Mensalidade);
    }

    [Fact]
    public void Validar_MensalidadeZero_DeveRetornarErro()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: 0m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("erro", registro.Status);
        var erro = Assert.Single(registro.Erros);
        Assert.Equal("Mensalidade", erro.Campo);
        Assert.Equal("A mensalidade deve ser maior que zero.", erro.Mensagem);
    }

    [Fact]
    public void Validar_MensalidadeNegativa_DeveRetornarErro()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: -100m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("erro", registro.Status);
        var erro = Assert.Single(registro.Erros);
        Assert.Equal("Mensalidade", erro.Campo);
        Assert.Equal("A mensalidade deve ser maior que zero.", erro.Mensagem);
    }

    [Fact]
    public void Validar_MensalidadeNaoNumerica_DeveRetornarErro()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: "abc");

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("erro", registro.Status);
        var erro = Assert.Single(registro.Erros);
        Assert.Equal("Mensalidade", erro.Campo);
        Assert.Equal("A mensalidade deve ser um valor numérico.", erro.Mensagem);
    }

    [Fact]
    public void Validar_RegistroComMultiplosErros_DeveRetornarTodosOsErros()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "123",
            nome: "",
            curso: "",
            dataNascimento: null,
            email: "aluno@email.com",
            mensalidade: 0m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("erro", registro.Status);
        Assert.Equal(5, registro.Erros.Count);
        Assert.Contains(registro.Erros, e => e.Campo == "Matrícula");
        Assert.Contains(registro.Erros, e => e.Campo == "Nome");
        Assert.Contains(registro.Erros, e => e.Campo == "Curso");
        Assert.Contains(registro.Erros, e => e.Campo == "Data de Nascimento");
        Assert.Contains(registro.Erros, e => e.Campo == "Mensalidade");
    }

    [Fact]
    public void Validar_FalhaNaTela2_DeveRetornarRegistrosVazio()
    {
        using var stream = CriarWorkbookComRegistros(9);

        var resultado = _service.Validar(stream);

        Assert.False(resultado.Sucesso);
        Assert.Empty(resultado.Registros);
    }

    [Fact]
    public void Validar_CabecalhoComAcentoUTF8_DeveAceitarMatriculaComAcento()
    {
        using var stream = CriarWorkbookComCabecalhos(CabecalhosOficiais, 10);

        var resultado = _service.Validar(stream);

        Assert.True(resultado.Sucesso);
        Assert.True(ObterValidacao(resultado, "estrutura").Sucesso);
    }

    [Fact]
    public void Validar_CabecalhoSemAcento_DeveFalharEstrutura()
    {
        var cabecalhosSemAcento = new[]
        {
            "Matricula*",
            "Nome*",
            "Curso*",
            "Data de Nascimento*",
            "Email",
            "Mensalidade"
        };

        using var stream = CriarWorkbookComCabecalhos(cabecalhosSemAcento, 10);

        var resultado = _service.Validar(stream);

        Assert.False(resultado.Sucesso);
        Assert.False(ObterValidacao(resultado, "estrutura").Sucesso);
    }

    [Fact]
    public void Validar_MensalidadeDecimal_DeveRetornarRepresentacaoInvariavel()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: 1090.50m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("1090.5", registro.Mensalidade);
    }

    [Fact]
    public void Validar_MensalidadeDecimal_SobCulturaPtBR_DeveRetornarRepresentacaoInvariavel()
    {
        var culturaOriginal = Thread.CurrentThread.CurrentCulture;
        var uiOriginal = Thread.CurrentThread.CurrentUICulture;

        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("pt-BR");

            using var stream = CriarWorkbookComRegistroEspecifico(
                matricula: "12345678",
                nome: "Aluno Teste",
                curso: "Curso A",
                dataNascimento: new DateTime(2000, 1, 1),
                email: "aluno@email.com",
                mensalidade: 1090.50m);

            var resultado = _service.Validar(stream);

            var registro = resultado.Registros[0];
            Assert.Equal("1090.5", registro.Mensalidade);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = culturaOriginal;
            Thread.CurrentThread.CurrentUICulture = uiOriginal;
        }
    }

    [Fact]
    public void Validar_MensalidadeInteira_DeveRetornarRepresentacaoInvariavel()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: 890m);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("890", registro.Mensalidade);
    }

    [Fact]
    public void Validar_MensalidadeTextualInvalido_DevePreservarTextoOriginal()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: "ABC");

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Equal("ABC", registro.Mensalidade);
    }

    [Fact]
    public void Validar_MensalidadeNula_DeveRetornarNull()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "12345678",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: null);

        var resultado = _service.Validar(stream);

        var registro = resultado.Registros[0];
        Assert.Null(registro.Mensalidade);
    }

    private static MemoryStream CriarWorkbookComRegistros(int quantidadeRegistros)
    {
        return CriarWorkbookComCabecalhos(CabecalhosOficiais, quantidadeRegistros);
    }

    private static MemoryStream CriarWorkbookComCabecalhos(string[] cabecalhos, int quantidadeRegistros)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Planilha1");

        for (var coluna = 0; coluna < cabecalhos.Length; coluna++)
        {
            worksheet.Cell(1, coluna + 1).Value = cabecalhos[coluna];
        }

        for (var linha = 0; linha < quantidadeRegistros; linha++)
        {
            worksheet.Cell(linha + 2, 1).Value = $"M{linha + 1:000}";
            worksheet.Cell(linha + 2, 2).Value = $"Aluno {linha + 1}";
            worksheet.Cell(linha + 2, 3).Value = "Curso A";
            worksheet.Cell(linha + 2, 4).Value = new DateTime(2000, 1, 1).AddDays(linha);
            worksheet.Cell(linha + 2, 5).Value = $"aluno{linha + 1}@email.com";
            worksheet.Cell(linha + 2, 6).Value = 500.00m;
        }

        return SalvarWorkbookEmMemoria(workbook);
    }

    private static MemoryStream CriarWorkbookComColunaExtra(int quantidadeRegistros)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Planilha1");

        for (var coluna = 0; coluna < CabecalhosOficiais.Length; coluna++)
        {
            worksheet.Cell(1, coluna + 1).Value = CabecalhosOficiais[coluna];
        }

        worksheet.Cell(1, CabecalhosOficiais.Length + 1).Value = "Coluna Extra";

        for (var linha = 0; linha < quantidadeRegistros; linha++)
        {
            worksheet.Cell(linha + 2, 1).Value = $"M{linha + 1:000}";
            worksheet.Cell(linha + 2, 2).Value = $"Aluno {linha + 1}";
            worksheet.Cell(linha + 2, 3).Value = "Curso A";
            worksheet.Cell(linha + 2, 4).Value = new DateTime(2000, 1, 1).AddDays(linha);
            worksheet.Cell(linha + 2, 5).Value = $"aluno{linha + 1}@email.com";
            worksheet.Cell(linha + 2, 6).Value = 500.00m;
            worksheet.Cell(linha + 2, 7).Value = "Extra";
        }

        return SalvarWorkbookEmMemoria(workbook);
    }

    private static MemoryStream CriarWorkbookComLinhasVaziasIntercaladas(int quantidadeRegistros)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Planilha1");

        for (var coluna = 0; coluna < CabecalhosOficiais.Length; coluna++)
        {
            worksheet.Cell(1, coluna + 1).Value = CabecalhosOficiais[coluna];
        }

        var linhaAtual = 2;
        for (var linha = 0; linha < quantidadeRegistros; linha++)
        {
            worksheet.Cell(linhaAtual, 1).Value = $"M{linha + 1:000}";
            worksheet.Cell(linhaAtual, 2).Value = $"Aluno {linha + 1}";
            worksheet.Cell(linhaAtual, 3).Value = "Curso A";
            worksheet.Cell(linhaAtual, 4).Value = new DateTime(2000, 1, 1).AddDays(linha);
            worksheet.Cell(linhaAtual, 5).Value = $"aluno{linha + 1}@email.com";
            worksheet.Cell(linhaAtual, 6).Value = 500.00m;
            linhaAtual += 2;
        }

        return SalvarWorkbookEmMemoria(workbook);
    }

    private static MemoryStream CriarWorkbookComCamposVaziosNasLinhas(int quantidadeRegistros)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Planilha1");

        for (var coluna = 0; coluna < CabecalhosOficiais.Length; coluna++)
        {
            worksheet.Cell(1, coluna + 1).Value = CabecalhosOficiais[coluna];
        }

        for (var linha = 0; linha < quantidadeRegistros; linha++)
        {
            worksheet.Cell(linha + 2, 1).Value = $"M{linha + 1:000}";
            worksheet.Cell(linha + 2, 2).Value = $"Aluno {linha + 1}";
            worksheet.Cell(linha + 2, 3).Value = "Curso A";
            worksheet.Cell(linha + 2, 4).Value = new DateTime(2000, 1, 1).AddDays(linha);
            worksheet.Cell(linha + 2, 5).Value = $"aluno{linha + 1}@email.com";
            worksheet.Cell(linha + 2, 6).Value = 500.00m;
        }

        return SalvarWorkbookEmMemoria(workbook);
    }

    private static MemoryStream SalvarWorkbookEmMemoria(XLWorkbook workbook)
    {
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private static ValidacaoItemResponse ObterValidacao(ValidacaoArquivoResponse resultado, string id)
    {
        return resultado.Validacoes.Single(v => v.Id == id);
    }

    private static MemoryStream CriarWorkbookComRegistroEspecifico(
        string? matricula,
        string? nome,
        string? curso,
        object? dataNascimento,
        string? email,
        object? mensalidade)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Planilha1");

        for (var coluna = 0; coluna < CabecalhosOficiais.Length; coluna++)
        {
            worksheet.Cell(1, coluna + 1).Value = CabecalhosOficiais[coluna];
        }

        if (matricula is not null) worksheet.Cell(2, 1).Value = matricula;
        if (nome is not null) worksheet.Cell(2, 2).Value = nome;
        if (curso is not null) worksheet.Cell(2, 3).Value = curso;
        if (dataNascimento is not null) worksheet.Cell(2, 4).Value = XLCellValue.FromObject(dataNascimento);
        if (email is not null) worksheet.Cell(2, 5).Value = email;
        if (mensalidade is not null) worksheet.Cell(2, 6).Value = XLCellValue.FromObject(mensalidade);

        for (var linha = 1; linha < 10; linha++)
        {
            var linhaAtual = linha + 2;
            worksheet.Cell(linhaAtual, 1).Value = $"1234567{linha}";
            worksheet.Cell(linhaAtual, 2).Value = $"Aluno {linha}";
            worksheet.Cell(linhaAtual, 3).Value = "Curso A";
            worksheet.Cell(linhaAtual, 4).Value = new DateTime(2000, 1, 1).AddDays(linha);
            worksheet.Cell(linhaAtual, 5).Value = $"aluno{linha}@email.com";
            worksheet.Cell(linhaAtual, 6).Value = 500.00m;
        }

        return SalvarWorkbookEmMemoria(workbook);
    }
}
