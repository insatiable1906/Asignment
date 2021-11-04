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
    public class PatientController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly string _entityName = "Patient";

        public PatientController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        // GET api/<PatientController>/
        [HttpGet]
        [Authorize(PermissionItem.User, PermissionAction.Read)]
        public ActionResult<List<Patient>> GetAllPatients()
        {
            List<Patient> list = new List<Patient>();
            var results = CacheActions.GetItems<Patient>(_memoryCache);
            foreach (object item in results.Select(i => i.Value))
            {
                if (item.GetType().Name == _entityName) list.Add(item as Patient);
            }
            return Ok(list);
        }

        //// GET api/<PatientController>/5
        [HttpGet("{id}")]
        [Authorize(PermissionItem.User, PermissionAction.Read)]
        public ActionResult<Patient> GetItem(Guid id)
        {
            //get specific patient
            return Ok(CacheActions.GetItem<Patient>(_memoryCache, id));
        }

        // GET: api/<PatientController>
        [HttpGet("GetSearch")]
        [Authorize(PermissionItem.User, PermissionAction.Read)]
        public ActionResult<IEnumerable<Patient>> GetPatientsPerLabResultsSearch(LabSearch lab)
        {
            //search for lab results
            return Ok(CacheActions.SearchLabResults(_memoryCache, lab));
        }

        // POST api/<PatientController>
        [HttpPost]
        [Authorize(PermissionItem.User, PermissionAction.Create)]
        public ActionResult<string> Post(Patient itemToInsert)
        {
            //add new patient
            itemToInsert.PatientId = Guid.NewGuid();
            var results = CacheActions.AddItem<Patient>(_memoryCache, itemToInsert.PatientId, itemToInsert);
            var res = Guid.TryParse(results, out Guid test);
            if (res)
                return Ok(results);
            else
                return new BadRequestObjectResult("Error Adding Lab report!!");
        }

        // PUT api/<PatientController>/5
        [HttpPut]
        [Authorize(PermissionItem.User, PermissionAction.Create)]
        public ActionResult<string> Put(Patient itemToUpdate)
        {
            //update patient
            var result = CacheActions.UpdateItem<Patient>(_memoryCache, itemToUpdate.PatientId, itemToUpdate);
            if (!string.IsNullOrEmpty(result))
                return Ok(result);
            else
                return new BadRequestObjectResult("Error Updating report!!"); //item didnt update
        }

        // DELETE api/<PatientController>/5
        [HttpDelete("{id}")]
        [Authorize(PermissionItem.User, PermissionAction.Create)]
        public ActionResult<bool> Delete(Guid id)
        {
            //Delete patients
            return Ok(CacheActions.RemoveItem(_memoryCache, id));  //return true or false if completed
        }
    }
}
