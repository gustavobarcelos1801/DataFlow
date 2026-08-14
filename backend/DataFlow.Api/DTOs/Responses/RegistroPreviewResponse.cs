namespace DataFlow.Api.DTOs.Responses;

public class RegistroPreviewResponse
{
    public string? Matricula { get; init; }
    public string? Nome { get; init; }
    public string? Curso { get; init; }
    public string? DataNascimento { get; init; }
    public string? Email { get; init; }
    public string? Mensalidade { get; init; }
    public required string Status { get; init; }
    public required IReadOnlyList<ErroRegistroResponse> Erros { get; init; }
}
