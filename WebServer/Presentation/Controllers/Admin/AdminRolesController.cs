using Application.Roles;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.Admin
{
    [ApiController]
    [Route("api/roles")]
    public sealed class AdminRolesController : ControllerBase
    {
        private readonly IRoleService _svc;
        private readonly ILogger<AdminRolesController> _logger;
        public AdminRolesController(IRoleService svc, ILogger<AdminRolesController> logger)
        { _svc = svc; _logger = logger; }

        // GET /api/roles?isActive=true&page=1&pageSize=50
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] bool? isActive, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
            => Ok(await _svc.ListAsync(isActive, page, pageSize, ct));

        // GET /api/roles/123
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id, CancellationToken ct = default)
            => await _svc.GetAsync(id, ct) is { } dto ? Ok(dto) : NotFound();

        // POST /api/roles
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest req, CancellationToken ct = default)
        {
            try
            {
                var created = await _svc.CreateAsync(req, ct);
                return CreatedAtAction(nameof(Get), new { id = created.RoleId }, created);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create Role failed");
                return Problem("Role 생성 실패", statusCode: 500);
            }
        }

        // PUT /api/roles/123
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleRequest req, CancellationToken ct = default)
        {
            try { await _svc.UpdateAsync(id, req, ct); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Role failed");
                return Problem("Role 수정 실패", statusCode: 500);
            }
        }

        // DELETE /api/roles/123
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            try { await _svc.DeleteAsync(id, ct); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete Role failed");
                return Problem("Role 삭제 실패", statusCode: 500);
            }
        }
    }
}
