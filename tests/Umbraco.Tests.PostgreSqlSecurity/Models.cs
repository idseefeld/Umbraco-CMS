namespace Umbraco.Cms.Tests.PostgreSqlSecurity;

public sealed record StepRecord(
    int Worker,
    int Iteration,
    string Step,
    DateTime StartedUtc,
    double Milliseconds,
    bool Success,
    string? ErrorType,
    string? ErrorMessage);

public sealed record StepStatistics(
    string Step,
    int Count,
    int Failures,
    double MeanMs,
    double MedianMs,
    double MinMs,
    double MaxMs,
    double P95Ms);

public sealed record RunInfo(
    DateTime StartedUtc,
    DateTime FinishedUtc,
    string Host,
    string Database,
    int ParallelTasks,
    int Iterations,
    double TotalMilliseconds);
