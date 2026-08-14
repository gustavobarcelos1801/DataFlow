namespace DataFlow.Api.DTOs.Responses;

public class ImportacaoJsonResponse
{
    public const int VersaoAtual = 1;
    public const string TipoOperacaoImportacao = "importacao";

    public required int Versao { get; init; }
    public required string TipoOperacao { get; init; }
    public required string GeradoEm { get; init; }
    public required ArquivoOrigemResponse ArquivoOrigem { get; init; }
    public required int QuantidadeRegistros { get; init; }
    public required IReadOnlyList<RegistroImportacaoJsonResponse> Registros { get; init; }
}