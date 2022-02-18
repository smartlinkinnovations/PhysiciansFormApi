namespace SmartTvApi.Model.Queries
{
    public class PatientAllergiesQueries
    {
        public string KrollPatientAlgTable1()
        {
            return @"
             SELECT AllergenGroup, AllergenDesc FROM [FDB].[dbo].[AllergenGroupMast] 
             WHERE AllergenGroup IN (@CODE_LIST)   
            ";
        }

        public string KrollPatientAlgTable2()
        {
            return @"
                SELECT ConceptId, Description FROM [FDB].[dbo].[AllergyPickList]
                WHERE @CONDITIONS                
            ";
        }
        
        public string KrollPatientAlgCodeNCodeType()
        {
            return @"
                SELECT PatID, Code, CodeType FROM [Pharmacy].[dbo].[PatAlg] WHERE PatID IN (
            	    SELECT ID FROM [Pharmacy].[dbo].[Pat] 
                    WHERE
                        Active = 1
                        AND NHID = @NH_ID
                        @CONDITION_WARD_LIST 
                        @CONDITION_PATIENT_LIST
                )
            ";
        }
    }
}