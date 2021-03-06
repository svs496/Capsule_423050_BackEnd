﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Contracts;
using Microsoft.AspNetCore.Mvc;


namespace TaskManager.API.Controllers
{
    //[Route("api/task")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        readonly ILoggerManager _logger;

        readonly IDataRepository<Entities.Task> _dataRepository;

        public TaskController(ILoggerManager logger, IDataRepository<Entities.Task> Repository)
        {
            _logger = logger;
            _dataRepository = Repository;
        }
        // GET api/Task
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                _logger.LogInfo("Inside Get All API Call.");
                IEnumerable<Entities.Task> tasks = _dataRepository.GetAll();
                return Ok(tasks);
            }
            catch (Exception ex)
            {

                _logger.LogError($"GetAll API Call failed: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetTasksByProjectId(long id)
        {
            try
            {
                _logger.LogInfo("Inside Get All API Call.");
                IEnumerable<Entities.Task> tasks = _dataRepository.GetListById(id);
                return Ok(tasks);
            }
            catch (Exception ex)
            {

                _logger.LogError($"GetAll API Call failed: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }


        // GET: api/Task/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                
                Entities.Task task = _dataRepository.Get(id);

                if (task == null)
                {
                    _logger.LogInfo($"Inside GetById : {id} not found");
                    return NotFound("The task record couldn't be found.");
                }

                return Ok(task);
            }
            catch (Exception ex)
            {

                _logger.LogError($"Get by Id API Call failed: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST api/Task
        [HttpPost]
        public IActionResult Post([FromBody] Entities.Task task)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                if (task == null)
                {
                    _logger.LogInfo($"Inside Post : Task is null");
                    return BadRequest("Task is null.");
                }

                //Map is needed as input param has taskId null
                Entities.Task newTaskToAdd = new Entities.Task
                {
                    EndDate = task.EndDate.HasValue ? task.EndDate.Value.Date : (DateTime?)null,
                    ParentTaskId = task.ParentTaskId,
                    Priority = task.Priority,
                    StartDate = task.StartDate.HasValue ? task.StartDate.Value.Date : (DateTime?)null,
                    Status = task.Status,
                    TaskName = task.TaskName.ToUpper(),
                    ProjectId = task.ProjectId,
                    UserId = task.UserId,
                    IsParentTask = task.IsParentTask
                };

                _dataRepository.Add(newTaskToAdd);
                return CreatedAtAction(nameof(Post), new { id = newTaskToAdd.TaskId }, newTaskToAdd);
            }
            catch (Exception ex)
            {
                
                _logger.LogError($"Post API Call failed: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT api/Task/5
        [HttpPut("{id}")]
        public IActionResult Put(long id, [FromBody] Entities.Task task)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (task == null || id != task.TaskId)
                {
                    _logger.LogInfo($"Inside Put : {id} not found");
                    return BadRequest("TaskId is null.");
                }
                
                _dataRepository.Update(task);
                return NoContent();
            }
            catch (Exception ex)
            {

                _logger.LogError($"Put API Call failed: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }



        // GET api/Task
        [HttpGet]
        public IActionResult GetParentTasks()
        {
            try
            {
                _logger.LogInfo("Inside GetParentTask.");
                IEnumerable<Entities.Task> parentTasks = _dataRepository.GetParentTasks();
                return Ok(parentTasks);
            }
            catch (Exception ex)
            {

                _logger.LogError($"GetParentTask API Call failed: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE api/Task/5
        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            try
            {

                Entities.Task task = _dataRepository.Get(id);

                if (task == null)
                {
                    _logger.LogInfo($"Inside Delete : {id} not found");
                    return NotFound("The Task record couldn't be found.");
                }


                // DO do not delete Task which has child
                if (_dataRepository.CanDeleteEntity(id))
                {
                    return Conflict(new { customMessage = $" Delete Conflict. Task  '{task.TaskName}' has child tasks." });
                }
                
                _dataRepository.Delete(task);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Delete API Call failed: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
