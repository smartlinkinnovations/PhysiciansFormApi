namespace SmartTvApi.Model.Queries
{
    public class ResponseQueries
    {
        public string KrollGetStatBoxIds()
        {
            return @"
                SELECT ID, 
                    CASE WHEN FirstName LIKE '%Narc%Stat%Box%' THEN 1 ELSE 0 END AS IsNarcotic
                FROM Pat 
                WHERE (NHID = 13 OR NHID = 14)  
                    AND (FirstName LIKE '%Stat%Box%YEARLY%' OR FirstName LIKE '%Narc%Stat%Box%')
                    AND LastName = @LAST_NAME                
            ";
        }

        public string KrollGetDrug()
        {
            return @"
                SELECT 
                    BrandName, Form, Strength, PriceGroup 
                FROM dbo.Drg 
                WHERE ID IN (
                    (
                        SELECT TOP 1 WITH TIES DrgID FROM dbo.Rx
                        WHERE Inactive = 0 AND PatID = @PAT_ID AND Sig @LIKE_CLAUSE @LIKE_CONDITION 
                        ORDER BY ROW_NUMBER() OVER (PARTITION BY OrigRxNum ORDER BY RxNum DESC)
                    )
                ) 
                UNION
                SELECT
                    Description AS BrandName, NULL AS Form, NULL AS Strength, PriceGroup
                FROM dbo.DrgMix
                WHERE ID IN (
                     (
                        SELECT TOP 1 WITH TIES MixID FROM dbo.Rx
                        WHERE Inactive = 0 AND PatID = @PAT_ID AND Sig @LIKE_CLAUSE @LIKE_CONDITION 
                        ORDER BY ROW_NUMBER() OVER (PARTITION BY OrigRxNum ORDER BY RxNum DESC)
                     )
                )                
                ORDER BY BrandName ASC
            ";
        }
    }
}