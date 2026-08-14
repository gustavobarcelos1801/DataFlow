namespace DataFlow.Api.DTOs.Responses;

public class ValidacaoArquivoResponse
{
    public bool Sucesso { get; init; }
    public required IReadOnlyList<ValidacaoItemResponse> Validacoes { get; init; }
    public required IReadOnlyList<RegistroPreviewResponse> Registros { get; init; }
}

