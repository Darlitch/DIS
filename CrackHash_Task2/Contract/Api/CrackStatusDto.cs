using Contract.Api.Enums;

namespace Contract.Api;

public record CrackStatusDto(RequestStatus RequestStatus, string[]? Data);