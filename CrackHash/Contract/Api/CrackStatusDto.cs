namespace Contract.Api;

public record CrackStatusDto(StatusEnum Status, string[]? Data);