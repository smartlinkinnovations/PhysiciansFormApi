using System.Collections.Generic;

namespace PhysiciansFormApi.Model.Data
{
    public class Response
    {
        public List<StatBox> Regular { get; set; }
        
        public List<StatBox> Narcotic { get; set; }
        
        public List<StatBox> Diabetic { get; set; }
        
        public List<StatBox> Palliative { get; set; }
    }
}