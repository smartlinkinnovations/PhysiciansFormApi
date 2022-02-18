using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.AspNetCore.Cors.Infrastructure;
using PhysiciansFormApi.Model.Data;
using SmartTvApi.Model.Queries;
using IDbConnection = PhysiciansFormApi.Model.Proto.IDbConnection;

namespace PhysiciansFormApi.Model.Services
{
    public class ResponseService: ResponseQueries
    {
        private readonly IDbConnection _connection;
        
        // suggested by Uditha, e.g. 27: OTC (COST +30%)
        private readonly IList<int> cashMedList = new List<int>(){27, 31, 32, 42, 43, 50, 51, 54, 56, 57, 58, 64, 66};
        
        public ResponseService(IDbConnection connection)
        {
            _connection = connection;
        }

        public Response Select(string nhName)
        {
            List<int> patListRegular = new List<int>();
            List<int> patListNarcotic = new List<int>();
            
            var response = new Response();
            nhName = nhName.Trim();
            using (SqlConnection con = new SqlConnection(_connection.ConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(KrollGetStatBoxIds(), con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@LAST_NAME", nhName);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader.GetInt32(1) == 0)
                            {
                                patListRegular.Add(reader.GetInt32(0));
                            }
                            else
                            {
                                patListNarcotic.Add(reader.GetInt32(0));
                            }
                        }
                    }
                }
            }
            // @build empty array response
            response.Regular = new List<StatBox>();
            response.Diabetic = new List<StatBox>();
            response.Narcotic = new List<StatBox>();
            response.Palliative = new List<StatBox>();
            
            if (patListRegular.Count > 0)
            {
                foreach (var patId in patListRegular)
                {
                    response.Regular = _getDrug(patId, "NOT LIKE", "'%DIABETIC%KIT%'");
                    response.Diabetic = _getDrug(patId, "LIKE", "'%DIABETIC%KIT%'");
                }
            }
            
            if (patListNarcotic.Count > 0)
            {
                foreach (var patId in patListNarcotic)
                {
                    response.Narcotic = _getDrug(patId, "NOT LIKE", "'%PALLIATIVE%KIT%'");
                    response.Palliative = _getDrug(patId, "LIKE", "'%PALLIATIVE%KIT%'");
                }
            }
            
            return response;
        }

        private List<StatBox> _getDrug(int PatId, string LIKE_CLAUSE, string LIKE_CONDITION)
        {
            List<StatBox> statBoxes = new List<StatBox>();

            using (SqlConnection con = new SqlConnection(_connection.ConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(KrollGetDrug(), con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@PAT_ID", PatId);
                    cmd.CommandText = cmd.CommandText.Replace("@LIKE_CLAUSE", LIKE_CLAUSE);
                    cmd.CommandText = cmd.CommandText.Replace("@LIKE_CONDITION", LIKE_CONDITION);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var _cashMed = "";
                            if (!reader.IsDBNull(3) && cashMedList.IndexOf(reader.GetInt32(3)) != -1)
                            {
                                _cashMed = "YES";
                            }

                            var _brandName = "";
                            if (!reader.IsDBNull(0))
                            {
                                _brandName = reader.GetString(0);
                            }
                            
                            var _medForm = "";
                            if (!reader.IsDBNull(1))
                            {
                                _medForm = reader.GetString(1);
                                _brandName += ", " + _medForm;
                            }

                            var _medStrength = "";
                            if (!reader.IsDBNull(2))
                            {
                                _medStrength = reader.GetString(2);
                                _brandName += " " + _medStrength;
                            }

                            StatBox statBox = new StatBox
                            {
                                BrandName = _brandName,
                                MedForm = _medForm,
                                MedStrength = _medStrength,
                                CashMed = _cashMed
                            };
                            statBoxes.Add(statBox);
                        }
                    }
                }
            }

            return new List<StatBox>(statBoxes);
        }
    }
}