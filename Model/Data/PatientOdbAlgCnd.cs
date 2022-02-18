using System.Collections.Generic;

namespace PhysiciansFormApi.Model.Data
{
    public class PatientOdbAlgCnd
    {
        public int Id { get; set; }
        
        public string ClientId { get; set; }
        
        public List<string> Allergies { get; set; }
        
        public List<string> Conditions { get; set; }
    }
}