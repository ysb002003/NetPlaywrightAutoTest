namespace AutoTest.Models;

public sealed record TestCaseItem(
    string Name,
    string Description,
    string Steps,
    string ExpectedResult,
    string ActualResult,
    string ExecutionStatus,
    string ExecutionTime,
    string Executor,
    string ExecutionResult);
