using System.Collections.Generic;

namespace PhysiciansFormApi.Model.Data
{
    public class RequestPatientAlgCnd
    {
        public int NhId { get; set; }
        
        public List<int> NhWardId { get; set; }
        
        public List<int> PatientId { get; set; }
    }
}