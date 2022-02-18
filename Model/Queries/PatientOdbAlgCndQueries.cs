namespace SmartTvApi.Model.Queries
{
    public class PatientOdbAlgCndQueries
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
        
        public string KrollPatientAlgList()
        {
            return @"
                SELECT AllergenGroup, AllergenDesc FROM [FDB].[dbo].[AllergenGroupMast] 
                WHERE AllergenGroup IN (
                    SELECT DISTINCT Code FROM [Pharmacy].[dbo].[PatAlg] WHERE PatID IN (
	                   SELECT ID FROM [Pharmacy].[dbo].[Pat] 
                        WHERE
                            Active = 1
                            AND NHID = @NH_ID
                            @CONDITION_WARD_LIST
                            @CONDITION_PATIENT_LIST 
	                    )
	                )                 
            ";
        }

        public string KrollPatientAlgCodeList()
        {
            return @"
                SELECT PatID, Code FROM [Pharmacy].[dbo].[PatAlg] WHERE PatID IN (
            	    SELECT ID FROM [Pharmacy].[dbo].[Pat] 
                    WHERE
                        Active = 1
                        AND NHID = @NH_ID
                        @CONDITION_WARD_LIST 
                        @CONDITION_PATIENT_LIST
                )
            ";
        }

        public string KrollPatientAlgCodeNCodeType()
        {
            return @"
                SELECT PatID, Code, CodeType, Severity FROM [Pharmacy].[dbo].[PatAlg] WHERE PatID IN (
            	    SELECT ID FROM [Pharmacy].[dbo].[Pat] 
                    WHERE
                        Active = 1
                        AND NHID = @NH_ID
                        @CONDITION_WARD_LIST 
                        @CONDITION_PATIENT_LIST
                )
            ";
        }

        public string KrollPatientCndList()
        {
            return @"
                SELECT Fdbdx, FdbdxDesc FROM [FDB].[dbo].[MedicalConditionsMast] 
                WHERE Fdbdx IN (
	                SELECT DISTINCT Code FROM [Pharmacy].[dbo].[PatCnd] WHERE PatID IN (
	                   SELECT ID FROM [Pharmacy].[dbo].[Pat] 
                        WHERE
                            Active = 1
                            AND NHID = @NH_ID
                            @CONDITION_WARD_LIST
                            @CONDITION_PATIENT_LIST
	                    )
                )
            ";
        }

        public string KrollPatientCndCodeList()
        {
            return @"
                SELECT PatID, Code FROM PatCnd WHERE PatID IN (
            	    SELECT ID FROM Pat 
                    WHERE
                        Active = 1
                        AND NHID = @NH_ID
                        @CONDITION_WARD_LIST
                        @CONDITION_PATIENT_LIST 
                )
            ";
        }

        public string KrollPatientCndByCode()
        {
            return @"
                SELECT Fdbdx as Code, FdbdxDesc AS Description 
                FROM [FDB].[dbo].[MedicalConditionsMast] 
                WHERE Fdbdx IN (@CODE_LIST) AND IsFDBDDCScreened = 1
                UNION
                SELECT ICD10CA As Code, Description 
                FROM FDB.dbo.ICD10CAMast 
                WHERE ICD10CA IN (@CODE_LIST) AND IsFDBDDCScreened = 1
                UNION
                SELECT ICD9CM AS Code, Description 
                FROM FDB.dbo.ICD9CMMast 
                WHERE ICD9CM IN (@CODE_LIST) AND IsFDBDDCScreened = 1
            ";
        }

        public string KrollPatientODB()
        {
            // @ ODB: SubPlan ID: 120
            return @"
                SELECT PatID, ClientID FROM PatPln WHERE SubPlanID = 120 AND PatID IN (
                    SELECT ID FROM Pat 
                        WHERE
                            Active = 1
                            AND NHID = @NH_ID
                            @CONDITION_WARD_LIST
                            @CONDITION_PATIENT_LIST 
                ) 
            ";
        }

        public string KrollAlgCndFromComments()
        {
            // CAST(CAST(Comment as VARBINARY(MAX)) as VARCHAR(MAX)) AS Comment
            return @"
                SELECT 
                    Topic, PatID,  
                    CASE 
                        WHEN CommentPlainText IS NOT NULL THEN CommentPlainText 
                        ELSE  CAST(CAST(Comment as VARBINARY(MAX)) as VARCHAR(MAX)) 
                    END AS Comment
                FROM [Pharmacy].[dbo].[PatCom]
                WHERE @CONDITIONS                    
            ";
        }
    }
}