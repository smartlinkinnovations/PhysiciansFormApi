using System.Collections.Generic;

namespace PhysiciansFormApi.Model.Data
{
    public class PatientCndCode
    {
        public int Id { get; set; }
        
        public List<CndCode> CndCodes { get; set; }
    }
    
    public class PatientCndList
    {
        public int Id { get; set; }
        public List<string> Conditions { get; set; }
    }
}