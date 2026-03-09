using Contract.Xml;
using Microsoft.AspNetCore.Mvc;
using Worker.Services;

namespace Worker.Controllers;

[ApiController]
[Route("/internal/api/[controller]/hash/crack")]
public class WorkerController(BruteForceService bruteForceService, CallbackService callbackService) : ControllerBase
{
    [HttpPost("task")]
    public async Task<IActionResult> CrackTask([FromBody] WorkerTaskRequest request)
    {
        var answers = bruteForceService.FindMatches(request);
        var response = new WorkerTaskResponse
        {
            RequestId = request.RequestId, 
            PartNumber = request.PartNumber,
            Answers = new Answers{Words = answers}
        };
        await callbackService.SendResultAsync(response);
        return Ok();
    }
}