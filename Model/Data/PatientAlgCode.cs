using System.Collections.Generic;

namespace PhysiciansFormApi.Model.Data
{
    public class PatientAlgCode
    {
        public int Id { get; set; }
        
        public List<AlgCode>  AlgCodes { get; set; }
    }

    public class PatientAlgList
    {
        public int Id { get; set; }
        public List<string> Allergies { get; set; }
    }
}