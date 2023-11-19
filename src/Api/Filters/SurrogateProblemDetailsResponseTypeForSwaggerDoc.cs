using System.Collections.Generic;

namespace Api.Filters;

public abstract class SurrogateProblemDetailsResponseTypeForSwaggerDoc
{
    public string? Type { get; set; }

    public string? Title { get; set; }

    public int? Status { get; set; }

    public string? Detail { get; set; }

    public string? Instance { get; set; }

    public Dictionary<string, ProblemDetail> Details { get; }
}
