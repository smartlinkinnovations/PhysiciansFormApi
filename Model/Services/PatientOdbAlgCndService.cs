using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using PhysiciansFormApi.Model.Data;
using SmartTvApi.Model.Queries;
using IDbConnection = PhysiciansFormApi.Model.Proto.IDbConnection;

namespace PhysiciansFormApi.Model.Services
{
    public class PatientOdbAlgCndService : PatientOdbAlgCndQueries
    {
        private const string TOPIC_CONDITIONS = "Conditions";
        private const string TOPIC_ALLERGIES = "Allergies";

        private readonly IDbConnection _connection;
        private readonly List<PatientOdbAlgCnd> _patientOdbAlgCnds = new List<PatientOdbAlgCnd>();

        public PatientOdbAlgCndService(IDbConnection connection)
        {
            _connection = connection;
        }

        public List<PatientOdbAlgCnd> List(RequestPatientAlgCnd requestPatientAlgCnd)
        {
            __PatientOdb(requestPatientAlgCnd);

            __AlgCndFromComments(requestPatientAlgCnd);

            __PatAllergies(requestPatientAlgCnd);

            __PatConditions(requestPatientAlgCnd);

            return _patientOdbAlgCnds;
        }

        public List<PatientOdbAlgCnd> ListWithTruncatedValues(RequestPatientAlgCnd requestPatientAlgCnd)
        {
            __PatientOdb(requestPatientAlgCnd);

            __AlgCndFromComments(requestPatientAlgCnd);

            __PatAllergies(requestPatientAlgCnd);

            __PatConditions(requestPatientAlgCnd);

            const int maxSizeAllergies = 220;
            const int maxSizeConditions = 300; //420;

            foreach (var row in _patientOdbAlgCnds)
            {
                var strConditions = string.Join(", ", row.Conditions);
                if (strConditions.Length > maxSizeConditions)
                {
                    row.Conditions = new List<string> { TruncateAtWord(strConditions, maxSizeConditions) };
                }

                var strAllergies = string.Join(", ", row.Allergies);
                if (strAllergies.Length > maxSizeAllergies)
                {
                    row.Allergies = new List<string> { TruncateAtWord(strAllergies, maxSizeAllergies) };
                }
            }

            return _patientOdbAlgCnds;
        }

        private void __PatientOdb(RequestPatientAlgCnd requestPatientAlgCnd)
        {
            using var connection = new SqlConnection(_connection.ConnectionString());
            using var cmd = new SqlCommand(KrollPatientODB(), connection);
            connection.Open();
            cmd.Parameters.AddWithValue("@NH_ID", requestPatientAlgCnd.NhId);

            cmd.CommandText = (requestPatientAlgCnd.NhWardId == null || requestPatientAlgCnd.NhWardId.Count > 0)
                ? cmd.CommandText.Replace("@CONDITION_WARD_LIST",
                    " AND NHWardID IN (" + string.Join(",", requestPatientAlgCnd.NhWardId) + ")")
                : cmd.CommandText.Replace("@CONDITION_WARD_LIST", "");

            cmd.CommandText = (requestPatientAlgCnd.PatientId == null ||
                               requestPatientAlgCnd.PatientId.Count > 0)
                ? cmd.CommandText.Replace("@CONDITION_PATIENT_LIST",
                    " AND PatID IN (" + string.Join(",", requestPatientAlgCnd.PatientId) + ")")
                : cmd.CommandText.Replace("@CONDITION_PATIENT_LIST", "");

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var patientOdbAlgCnd = new PatientOdbAlgCnd
                    {
                        Id = reader.GetInt32(0),
                        ClientId = reader.GetString(1),
                        Allergies = new List<string>(),
                        Conditions = new List<string>()
                    };
                    _patientOdbAlgCnds.Add(patientOdbAlgCnd);
                }
            }
        }

        private void __AlgCndFromComments(RequestPatientAlgCnd requestPatientAlgCnd)
        {
            using (var connection = new SqlConnection(_connection.ConnectionString()))
            {
                using (var cmd = new SqlCommand(KrollAlgCndFromComments(), connection))
                {
                    // @build conditions
                    var conditions = " (Topic = '" + TOPIC_CONDITIONS + "' OR Topic = '" + TOPIC_ALLERGIES + "')";
                    conditions += " AND PatID IN (" + string.Join(", ", requestPatientAlgCnd.PatientId) + ")";

                    connection.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = cmd.CommandText.Replace("@CONDITIONS", conditions);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var _topic = reader.GetString(0);
                            var _patId = reader.GetInt32(1);
                            var _comment = reader.GetString(2).Trim();

                            var obj = _patientOdbAlgCnds.Find(x => x.Id == _patId);
                            if (obj != null)
                            {
                                if (_topic == TOPIC_ALLERGIES)
                                {
                                    obj.Allergies.Add(_comment);
                                }
                                else if (_topic == TOPIC_CONDITIONS)
                                {
                                    obj.Conditions.Add(_comment);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void __PatConditions(RequestPatientAlgCnd requestPatientAlgCnd)
        {
            var cndCodeList = new List<string>();

            var patientCndCodesList = new List<PatientCodeList>();

            using (var con = new SqlConnection(_connection.ConnectionString()))
            {
                using (var cmd = new SqlCommand(KrollPatientCndCodeList(), con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@NH_ID", requestPatientAlgCnd.NhId);

                    cmd.CommandText = (requestPatientAlgCnd.NhWardId == null || requestPatientAlgCnd.NhWardId.Count > 0)
                        ? cmd.CommandText.Replace("@CONDITION_WARD_LIST",
                            " AND NHWardID IN (" + string.Join(",", requestPatientAlgCnd.NhWardId) + ")")
                        : cmd.CommandText.Replace("@CONDITION_WARD_LIST", "");

                    cmd.CommandText = (requestPatientAlgCnd.PatientId == null ||
                                       requestPatientAlgCnd.PatientId.Count > 0)
                        ? cmd.CommandText.Replace("@CONDITION_PATIENT_LIST",
                            " AND PatID IN (" + string.Join(",", requestPatientAlgCnd.PatientId) + ")")
                        : cmd.CommandText.Replace("@CONDITION_PATIENT_LIST", "");


                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var patId = reader.GetInt32(0);
                            var patCode = reader.GetString(1);

                            cndCodeList.Add(patCode);

                            var patCndCodesListIndex = patientCndCodesList.FindIndex(x => x.Id == patId);
                            if (patCndCodesListIndex != -1)
                            {
                                patientCndCodesList[patCndCodesListIndex].Code.Add(patCode);
                            }
                            else
                            {
                                PatientCodeList patientCodeList = new PatientCodeList
                                {
                                    Id = patId,
                                    Code = new List<string>
                                    {
                                        patCode
                                    }
                                };

                                patientCndCodesList.Add(patientCodeList);
                            }
                        }
                    }
                }
            }

            if (cndCodeList.Count > 0)
            {
                var cndCodes = new List<CndCode>();
                // @
                using (var connection = new SqlConnection(_connection.ConnectionString()))
                {
                    using (var cmd = new SqlCommand(KrollPatientCndByCode(), connection))
                    {
                        connection.Open();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText =
                            cmd.CommandText.Replace("@CODE_LIST", "'" + string.Join("','", cndCodeList) + "'");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cndCodes.Add(
                                    new CndCode
                                    {
                                        Code = reader.GetString(0),
                                        Description = reader.GetString(1)
                                    }
                                );
                            }
                        }
                    }
                }

                //
                foreach (var rows in patientCndCodesList)
                {
                    /// var patientCndList = new PatientCndList {Id = rows.Id};
                    var _cndList = (from _code in rows.Code
                        select cndCodes.Find(x => x.Code == _code)
                        into ob
                        where ob != null
                        select ob.Description).ToList();

                    var obj = _patientOdbAlgCnds.Find(x => x.Id == rows.Id);
                    if (obj != null)
                    {
                        var _previousList = obj.Conditions;
                        obj.Conditions = _previousList.Concat(_cndList).ToList();
                    }
                }
            }
        }

        private void __PatAllergies(RequestPatientAlgCnd requestPatientAlgCnd)
        {
            // @fetch patient alg codes
            var _patAlgCodes = PatientAlgCodes(requestPatientAlgCnd);

            var patientAlgLists = new List<PatientAlgList>();

            var _conditionForTable1 = new List<int>();
            var _conditionForTable2 = new List<string>();
            if (_patAlgCodes.Count > 0)
            {
                // @build conditions list for Table 1 and Table 2
                foreach (var algCode in _patAlgCodes)
                {
                    if (algCode.AlgCodes.Count > 0)
                    {
                        foreach (var row in algCode.AlgCodes)
                        {
                            _conditionForTable1.Add(row.Code);
                            _conditionForTable2.Add(" (ConceptId = " + row.Code + " AND ConceptIdType = " +
                                                    row.CodeType + ")");
                        }
                    }
                }


                // @fetch Allergies from Table 1
                var algTable1 = PatAllergiesTable1(_conditionForTable1);
                // @fetch allergies from Table 2
                var algTable2 = PatAllergiesTable2(_conditionForTable2);
                // @combine Allergies from Table 1 + Table 2
                var patAllergyList = algTable1.Union(algTable2).Distinct().ToList();
                // @build unique Patient Allergy List
                var uniquePatientAlgList = new List<AlgCode>();
                uniquePatientAlgList = patAllergyList.GroupBy(x => x.Code).Select(x => x.Last()).ToList();

                /// uniquePatientAlgList = patAllergyList;

                if (uniquePatientAlgList.Count > 0)
                {
                    foreach (var row in _patAlgCodes)
                    {
                        /// var patientAlgList = new PatientAlgList {Id = row.Id};
                        var algList = new List<string>();
                        foreach (var r in row.AlgCodes)
                        {
                            var ob = uniquePatientAlgList.Find(x => x.Code == r.Code);
                            if (ob != null)
                            {
                                algList.Add(ob.Description);
                            }
                        }

                        /// patientAlgList.Allergies = algList;
                        /// patientAlgLists.Add(patientAlgList);

                        var obj = _patientOdbAlgCnds.Find(x => x.Id == row.Id);
                        if (obj != null)
                        {
                            var _previousList = obj.Allergies;
                            obj.Allergies = _previousList.Concat(algList).ToList();
                        }
                    }
                }
            }
        }

        private List<PatientAlgCode> PatientAlgCodes(RequestPatientAlgCnd requestPatientAlgCnd)
        {
            var patientAlgCodes = new List<PatientAlgCode>();
            using (var con = new SqlConnection(_connection.ConnectionString()))
            {
                using (var cmd = new SqlCommand(KrollPatientAlgCodeNCodeType(), con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@NH_ID", requestPatientAlgCnd.NhId);
                    cmd.CommandText = (requestPatientAlgCnd.NhWardId == null || requestPatientAlgCnd.NhWardId.Count > 0)
                        ? cmd.CommandText.Replace("@CONDITION_WARD_LIST",
                            " AND NHWardID IN (" + string.Join(",", requestPatientAlgCnd.NhWardId) + ")")
                        : cmd.CommandText.Replace("@CONDITION_WARD_LIST", "");
                    cmd.CommandText = (requestPatientAlgCnd.PatientId == null ||
                                       requestPatientAlgCnd.PatientId.Count > 0)
                        ? cmd.CommandText.Replace("@CONDITION_PATIENT_LIST",
                            " AND PatID IN (" + string.Join(",", requestPatientAlgCnd.PatientId) + ")")
                        : cmd.CommandText.Replace("@CONDITION_PATIENT_LIST", "");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var patId = reader.GetInt32(0);
                            var algCode = int.Parse(reader.GetString(1));
                            var algCodeType = reader.GetInt32(2);
                            var severity = reader.IsDBNull(3) ? "" : reader.GetString(3);

                            var _algCode = new AlgCode
                            {
                                Code = algCode,
                                CodeType = algCodeType,
                                Severity = severity
                            };

                            var _index = patientAlgCodes.FindIndex(x => x.Id == patId);
                            if (_index != -1)
                            {
                                patientAlgCodes[_index].AlgCodes.Add(_algCode);
                            }
                            else
                            {
                                patientAlgCodes.Add(new PatientAlgCode
                                {
                                    Id = patId,
                                    AlgCodes = new List<AlgCode>
                                    {
                                        _algCode
                                    }
                                });
                            }
                        }
                    }
                }
            }

            return patientAlgCodes;
        }

        private List<AlgCode> PatAllergiesTable1(IEnumerable<int> conditions)
        {
            var algCodes = new List<AlgCode>();
            using (var connection = new SqlConnection(_connection.ConnectionString()))
            {
                using (var command = new SqlCommand(KrollPatientAlgTable1(), connection))
                {
                    connection.Open();
                    command.CommandType = CommandType.Text;
                    command.CommandText = command.CommandText.Replace("@CODE_LIST", string.Join(",", conditions));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var algCode = new AlgCode
                            {
                                Code = reader.GetInt32(0),
                                Description = reader.GetString(1)
                            };
                            algCodes.Add(algCode);
                        }
                    }
                }
            }

            return algCodes;
        }

        private List<AlgCode> PatAllergiesTable2(IEnumerable<string> conditions)
        {
            
            
            var algCodes = new List<AlgCode>();

            using (var connection = new SqlConnection(_connection.ConnectionString()))
            {
                using (var command = new SqlCommand(KrollPatientAlgTable2(), connection))
                {
                    connection.Open();
                    command.CommandType = CommandType.Text;
                    command.CommandText = command.CommandText.Replace("@CONDITIONS", string.Join("OR", conditions));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var algCode = new AlgCode
                            {
                                Code = reader.GetInt32(0),
                                Description = reader.GetString(1)
                            };
                            algCodes.Add(algCode);
                        }
                    }
                }
            }

            return algCodes;
        }

        private static string TruncateAtWord(string value, int length)
        {
            if (value == null || value.Trim().Length <= length)
                return value;

            var index = value.Trim().LastIndexOf(" ");

            while ((index + 3) > length)
                index = value.Substring(0, index).Trim().LastIndexOf(" ");

            if (index > 0)
                return value.Substring(0, index) + " ...more in patient profile";

            return value.Substring(0, length - 3) + " ...more in patient profile";
        }
    }
}