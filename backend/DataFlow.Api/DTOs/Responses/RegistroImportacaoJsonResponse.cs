namespace DataFlow.Api.DTOs.Responses;

public class RegistroImportacaoJsonResponse
{
    public required string Matricula { get; init; }
    public required string Nome { get; init; }
    public required string Curso { get; init; }
    public required string DataNascimento { get; init; }
    public string? Email { get; init; }
    public decimal? Mensalidade { get; init; }
}