using System.Collections.Generic;

namespace PhysiciansFormApi.Model.Data
{
    public class PatientCodeList
    {
        public int Id { get; set; }
        public List<string> Code { get; set; }
    }
}