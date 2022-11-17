using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VillaApiController : ControllerBase
{
    private readonly ILogging _logger;
    private readonly IMapper _mapper;
    private readonly IVillaRepository _dbVilla;

    public VillaApiController(ILogging logger, IMapper mapper, IVillaRepository dbVilla)
    {
        _logger = logger;
        _mapper = mapper;
        _dbVilla = dbVilla;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
    {
        IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();
        _logger.Log("Getting all villas", "success");
        return Ok(_mapper.Map<List<VillaDTO>>(villaList));
    }

    [HttpGet("{id:int}", Name = "GetVilla")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VillaDTO>> GetVilla(int id)
    {
        if (id == 0)
        {
            _logger.Log("Get Villa Error with Id: " + id, "error");
            return BadRequest();
        }

        var villa = await _dbVilla.GetAsync(u => u.Id == id);
        if (villa == null)
        {
            return NotFound();
        }

        _logger.Log("Getting villa", "success");
        return Ok(_mapper.Map<VillaDTO>(villa));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VillaDTO>> CreateVilla([FromBody] VillaCreateDTO villaCreateDto)
    {
        if (await _dbVilla.GetAsync(u => u.Name.ToLower() == villaCreateDto.Name.ToLower()) != null)
        {
            ModelState.AddModelError("CustomeError", "Villa already Exist");
            return BadRequest(ModelState);
        }

        if (villaCreateDto == null)
        {
            return BadRequest(villaCreateDto);
        }

        Villa model = _mapper.Map<Villa>(villaCreateDto);
        await _dbVilla.CreateAsync(model);

        return CreatedAtRoute("GetVilla", new { id = model.Id }, model);
    }

    [HttpDelete("{id:int}", Name = "DeleteVilla")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteVilla(int id)
    {
        if (id == 0)
        {
            return BadRequest();
        }

        var villa = await _dbVilla.GetAsync(u => u.Id == id);
        if (villa == null)
        {
            return NotFound();
        }

        _dbVilla.RemoveAsync(villa);
        return NoContent();
    }

    [HttpPut("{id:int}", Name = "UpdateVilla")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO villaUpdateDto)
    {
        if (villaUpdateDto == null || id != villaUpdateDto.Id)
        {
            return BadRequest();
        }

        Villa model = _mapper.Map<Villa>(villaUpdateDto);

        await _dbVilla.UpdateAsync(model);
        return NoContent();
    }

    [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDto)
    {
        if (patchDto == null || id == 0)
        {
            return BadRequest();
        }

        var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked:false);
        if (villa == null)
        {
            return BadRequest();
        }

        VillaUpdateDTO villaUpdateDto = _mapper.Map<VillaUpdateDTO>(villa);
        patchDto.ApplyTo(villaUpdateDto, ModelState);
        Villa model = _mapper.Map<Villa>(villaUpdateDto);

        await _dbVilla.UpdateAsync(model);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return NoContent();
    }
}