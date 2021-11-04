using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using test2.ActionFilters;
using test2.Models;
using test2.Utilities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace test2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabResultsController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly string _entityName = "LabResults";

        public LabResultsController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        // GET: api/<LabResultsController>
        [HttpGet]
        [Authorize(PermissionItem.User, PermissionAction.Read)]
        public ActionResult<LabResults> GetAllReports()
        {
            List<LabResults> list = new List<LabResults>();
            var results = CacheActions.GetItems<LabResults>(_memoryCache);
            foreach (object item in results.Select(i => i.Value))
            {
                if (item.GetType().Name == _entityName) list.Add(item as LabResults);
            }
            return Ok(list);
        }

        // GET: api/<LabResultsController>
        [HttpGet("{id}")]
        [Authorize(PermissionItem.User, PermissionAction.Read)]
        public ActionResult<LabResults> GetReport(Guid id)
        {
            return Ok(CacheActions.GetItem<LabResults>(_memoryCache, id));
        }

        // POST api/<LabResultsController>
        [HttpPost]
        [Authorize(PermissionItem.User, PermissionAction.Create)]
        public ActionResult<string> Post(LabResults itemToInsert)
        {
            //patient id is required
            if (String.IsNullOrEmpty(itemToInsert.PatientID.ToString()) || itemToInsert.PatientID.ToString() == "00000000-0000-0000-0000-000000000000") return new BadRequestObjectResult("PatientID is required!!");

            //add new entry
            itemToInsert.LabID = Guid.NewGuid();
            var results = CacheActions.AddItem(_memoryCache, itemToInsert.LabID, itemToInsert);
            var res = Guid.TryParse(results, out Guid test);
            if (res)
                return Ok(results);
            else
                return new BadRequestObjectResult("Error Adding Lab report!!");
        }

        // PUT api/<LabResultsController>/5
        [HttpPut]
        [Authorize(PermissionItem.User, PermissionAction.Create)]
        public ActionResult<string> Put(LabResults itemToUpdate)
        {
            //update
            var result = CacheActions.UpdateItem<LabResults>(_memoryCache, itemToUpdate.LabID, itemToUpdate);
            if (!string.IsNullOrEmpty(result))
                return Ok(result);
            else
                return new BadRequestObjectResult("Error Updating report!!"); //item didnt update
        }

        // DELETE api/<LabResultsController>/5
        [HttpDelete("{id}")]
        [Authorize(PermissionItem.User, PermissionAction.Create)]
        public ActionResult<bool> Delete(Guid id)
        {
            return Ok(CacheActions.RemoveItem(_memoryCache, id));  //return true or false if completed
        }
    }
}
