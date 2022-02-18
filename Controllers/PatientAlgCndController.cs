using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PhysiciansFormApi.Model.Connections;
using PhysiciansFormApi.Model.Data;
using PhysiciansFormApi.Model.Services;

namespace PhysiciansFormApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientAlgCndController : Controller
    {
        [HttpPost("AlgCnd")]
        public ActionResult<List<PatientOdbAlgCnd>> AlgCnd(RequestPatientAlgCnd requestPatientAlgCnd)
        {
            var _dbService = new PatientOdbAlgCndService(new KrollPharmacyConnection());
            return _dbService.ListWithTruncatedValues(requestPatientAlgCnd);
        }
        
        [HttpPost("AllergiesConditionsByPatient")]
        public ActionResult<List<PatientOdbAlgCnd>> AllergiesConditionsByPatient(RequestPatientAlgCnd requestPatientAlgCnd)
        {
            var _dbService = new PatientOdbAlgCndService(new KrollPharmacyConnection());
            var data = _dbService.List(requestPatientAlgCnd);
            foreach (var row in data)
            {
                if (row.Allergies.Count > 0)
                {
                    row.Allergies = row.Allergies.OrderBy(x => x).ToList();
                }

                if (row.Conditions.Count > 0)
                {
                    row.Conditions = row.Conditions.OrderBy(x => x).ToList();
                }
            }
            return data;
        }
    }
}