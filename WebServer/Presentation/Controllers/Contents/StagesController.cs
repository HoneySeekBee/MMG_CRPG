namespace WebServer.Controllers
{
    using Application.Common.Models;
    using Application.Contents.Stages;
    using Microsoft.AspNetCore.Mvc;

    namespace WebServer.Controllers
    {
        [ApiController]
        [Route("api/stages")]
        public sealed class StagesController : ControllerBase
        {
            private readonly IStagesService _svc;
            public StagesController(IStagesService svc) => _svc = svc;

            // GET /api/stages
            [HttpGet]
            [ProducesResponseType(typeof(PagedResult<StageSummaryDto>), 200)]
            public async Task<ActionResult<PagedResult<StageSummaryDto>>> GetList(
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 20,
                [FromQuery] int? chapter = null,
                [FromQuery] bool? isActive = null,
                [FromQuery] string? search = null,
                CancellationToken ct = default)
            {
                var filter = new StageListFilter(chapter, isActive, search, page, pageSize);
                var result = await _svc.GetListAsync(filter, ct);
                return Ok(result);
            }

            // GET /api/stages/{id}
            [HttpGet("{id:int}")]
            [ProducesResponseType(typeof(StageDetailDto), 200)]
            [ProducesResponseType(404)]
            public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
            {
                try
                {
                    var dto = await _svc.GetDetailAsync(id, ct);
                    if (dto is null) return NotFound();
                    return Ok(dto);
                }
                catch (Exception ex)
                {
                    return Problem(
                        statusCode: 500,
                        title: "Get stage failed",
                        detail: ex.Message
                    );
                }
            }

            // POST /api/stages
            [HttpPost]
            [ProducesResponseType(typeof(object), 201)]
            [ProducesResponseType(400)]
            public async Task<IActionResult> Create([FromBody] CreateStageRequest req, CancellationToken ct = default)
            {
                try
                {
                    var id = await _svc.CreateAsync(req, ct);
                    return CreatedAtAction(nameof(GetById), new { id }, new { id });
                }
                catch (ArgumentException ex) // 유효성 문제
                {
                    return ValidationProblem(detail: ex.Message, statusCode: 400);
                }
            }

            // PUT /api/stages/{id}
            [HttpPut("{id:int}")]
            [ProducesResponseType(204)]
            [ProducesResponseType(400)]
            [ProducesResponseType(404)]
            public async Task<IActionResult> Update(int id, [FromBody] UpdateStageRequest req, CancellationToken ct = default)
            {
                try
                {
                    // 서비스가 id를 별도로 받는 시그니처라면 이 라인 사용:
                    await _svc.UpdateAsync(id, req, ct);

                    // 서비스가 req.Id를 사용한다면, 아래처럼 일치 검증만 하고 넘겨도 됨:
                    // if (req.Id != id) return ValidationProblem(detail: "ID_MISMATCH", statusCode: 400);
                    // await _svc.UpdateAsync(req, ct);

                    return NoContent();
                }
                catch (InvalidOperationException) // not found
                {
                    return NotFound();
                }
                catch (ArgumentException ex) // validation
                {
                    return ValidationProblem(detail: ex.Message, statusCode: 400);
                }
            }

            // DELETE /api/stages/{id}
            [HttpDelete("{id:int}")]
            [ProducesResponseType(204)]
            [ProducesResponseType(404)]
            public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
            {
                try
                {
                    await _svc.DeleteAsync(id, ct);
                    return NoContent();
                }
                catch (InvalidOperationException)
                {
                    return NotFound();
                }
            }
        }
    }

}
