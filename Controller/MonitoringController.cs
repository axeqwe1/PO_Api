using System.Data;
using Azure.Core;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PO_Api.Data;
using PO_Api.Data.DTO;
using PO_Api.Data.DTO.Request;
using PO_Api.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PO_Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonitoringController : ControllerBase
    {

        private readonly AppDbContext _db;
        private readonly JwtService _jwt;
        private readonly EmailService _emailService;
        public MonitoringController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
            _emailService = new EmailService(
                host: "mail.yehpattana.com",
                port: 587,
                username: "jarvis-ypt@Ta-yeh.com",
                password: "J!@#1028", // ต้องเป็น App Password
                fromEmail: "jarvis-ypt@ta-yeh.com",
                displayFromEmail: "YPT PO ระบบสมาชิก"
            );
        }




        [HttpPost("GetMoniData")]
        public async Task<IActionResult> GetData(MonitoringRequest request)
        {
            try
            {
                string formattedDate = request.date.ToString("MM/dd/yyyy");

                using (var connection = _db.Database.GetDbConnection())
                {
                    if (connection.State != ConnectionState.Open)
                        await connection.OpenAsync();
                    string comp = "";
                    if (request.Company == "YPT") comp = "Y";
                    if (request.Company == "GNX") comp = "G";
                    var parameters = new DynamicParameters();
                    parameters.Add("@date", formattedDate, DbType.String, size: 10);
                    parameters.Add("@fac", comp, DbType.String, size: 1000);
                    parameters.Add("@linetype", 0, DbType.Int16);

                    var query = await connection.QueryAsync<Monitoring>(
                        "Netgarment.dbo.sp_MonitoringLineDetail",
                        param: parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    var results = query;
                    //.Where(t => !t.LineGroup.Contains("SP") && !t.LineGroup.Contains("Supplier"));
                    var excludeGroups = new[] { "SP1(SAMPLE)", "Sub", "Sample" };

                    var defect = _db.MonitorLineStatus.Include(t => t.MasterLine);
                    var QC = _db.WorkMinutes.Include(t => t.MasterLine);
                    var MinuteDay = _db.WorkMinutes.Include(t => t.MasterLine);
                    var MinuteAct = _db.WorkMinutes.Include(t => t.MasterLine);
                    var Sewer = _db.MonitorLineStatus.Include(t => t.MasterLine);
                    var OT = _db.MonitorLineStatus.Include(t => t.MasterLine);
                    decimal defectVal = 0, qcVal = 0, minDayVal = 0, minActVal = 0, sewerVal = 0, otVal = 0;

                    if (request.mode == "All")
                    {
                        results = query;
                        defectVal = await defect
                            .Where(t => t.DateQC == results.First().DateQC && t.Line.Contains(comp))
                            .SumAsync(t => (decimal)t.QtyDefect);

                        qcVal = await QC
                            .Where(t => t.DateQc == results.First().DateQC && t.line.Contains(comp))
                            .SumAsync(t => (decimal)t.QtyQC);

                        minActVal = await MinuteAct
                            .Where(t => t.DateQc == results.First().DateQC && t.line.Contains(comp))
                            .SumAsync(t => (decimal)t.Minute);

                        minDayVal = await MinuteDay
                            .Where(t => t.DateQc == results.First().DateQC && t.line.Contains(comp))
                            .SumAsync(t => (decimal)t.MinuteDay);

                        sewerVal = await Sewer
                            .Where(t => t.DateQC == results.First().DateQC && t.Line.Contains(comp))
                            .SumAsync(t => (decimal)t.Worker);

                        otVal = await OT
                            .Where(t =>
                                t.DateQC == results.First().DateQC &&
                                t.Line.Contains(comp) )
                            .SumAsync(t => (decimal)t.WorkerOT);
                    }
                    else if (request.mode == "Normal")
                    {
                        results = query.Where(t => !t.LineGroup.Contains("SP") && !t.LineGroup.Contains("Supplier") && !t.LineGroup.Contains("Sample")).ToList();
                        defectVal = await defect
                            .Where(t =>
                                t.DateQC == results.First().DateQC &&
                                t.Line.Contains(comp) &&
                                !t.MasterLine.NameGroup.Contains("SP1(SAMPLE)") &&
                                !t.MasterLine.NameGroup.Contains("Sub") &&
                                !t.MasterLine.NameGroup.Contains("Sample"))
                            .SumAsync(t => (decimal)t.QtyDefect);

                        qcVal = await QC
                            .Where(t =>
                                t.DateQc == results.First().DateQC &&
                                t.line.Contains(comp) &&
                                !t.MasterLine.NameGroup.Contains("SP1(SAMPLE)") &&
                                !t.MasterLine.NameGroup.Contains("Sub") &&
                                !t.MasterLine.NameGroup.Contains("Sample"))
                            .SumAsync(t => (decimal)t.QtyQC);

                        minActVal = await MinuteAct
                            .Where(t =>
                                t.DateQc == results.First().DateQC &&
                                t.line.Contains(        comp) &&
                                !t.MasterLine.NameGroup.Contains("SP1(SAMPLE)") &&
                                !t.MasterLine.NameGroup.Contains("Sub") &&
                                !t.MasterLine.NameGroup.Contains("Sample"))
                            .SumAsync(t => (decimal)t.Minute);

                        minDayVal = await MinuteDay
                            .Where(t =>
                                t.DateQc == results.First().DateQC &&
                                t.line.Contains(comp) &&
                                !t.MasterLine.NameGroup.Contains("SP1(SAMPLE)") &&
                                !t.MasterLine.NameGroup.Contains("Sub") &&
                                !t.MasterLine.NameGroup.Contains("Sample"))
                            .SumAsync(t => (decimal)t.MinuteDay);

                        sewerVal = await Sewer
                            .Where(t =>
                                t.DateQC == results.First().DateQC &&
                                t.Line.Contains(comp) &&
                                !t.MasterLine.NameGroup.Contains("SP1(SAMPLE)") &&
                                !t.MasterLine.NameGroup.Contains("Sub") &&
                                !t.MasterLine.NameGroup.Contains("Sample"))
                            .SumAsync(t => (decimal)t.Worker);

                        otVal = await OT
                            .Where(t =>
                                t.DateQC == results.First().DateQC &&
                                t.Line.Contains(comp) &&
                                !t.MasterLine.NameGroup.Contains("SP1(SAMPLE)") &&
                                !t.MasterLine.NameGroup.Contains("Sub") &&
                                !t.MasterLine.NameGroup.Contains("Sample"))
                            .SumAsync(t => (decimal)t.WorkerOT);
                    }
                    else // SAMPLE
                    {
                        results = query.Where(t => t.LineGroup.Contains("SP1") || t.LineGroup.Contains("Supplier") || t.LineGroup.Contains("Sample")).ToList();

                        if (results.Count() == 0)
                        {
                            defectVal = 0;
                            qcVal = 0;
                            minActVal = 0;
                            minDayVal = 0;
                            sewerVal = 0;
                            otVal = 0;
                        }
                        else
                        {
                            var targetDate = results.First().DateQC;

                            defectVal = await defect
                                .Where(t =>
                                    t.DateQC == targetDate &&
                                    t.Line.Contains(comp) &&
                                    (
                                    t.MasterLine.NameGroup.Contains("SP1(SAMPLE)") ||
                                    t.MasterLine.NameGroup.Contains("Sub") ||
                                    t.MasterLine.NameGroup.Contains("Sample"))
                                    )
                                .SumAsync(t => (decimal)t.QtyDefect);

                            qcVal = await QC
                                .Where(t =>
                                    t.DateQc == targetDate &&
                                    t.line.Contains(comp) &&
                                    (
                                    t.MasterLine.NameGroup.Contains("SP1(SAMPLE)") ||
                                    t.MasterLine.NameGroup.Contains("Sub") ||
                                    t.MasterLine.NameGroup.Contains("Sample"))
                                    )
                                .SumAsync(t => (decimal)t.QtyQC);

                            minActVal = await MinuteAct
                                .Where(t =>
                                    t.DateQc == targetDate &&
                                    t.line.Contains(comp) &&
                                    (
                                    t.MasterLine.NameGroup.Contains("SP1(SAMPLE)") ||
                                    t.MasterLine.NameGroup.Contains("Sub") ||
                                    t.MasterLine.NameGroup.Contains("Sample"))
                                    )
                                .SumAsync(t => (decimal)t.Minute);

                            minDayVal = await MinuteDay
                                .Where(t =>
                                    t.DateQc == targetDate &&
                                    t.line.Contains(comp) &&
                                    (
                                    t.MasterLine.NameGroup.Contains("SP1(SAMPLE)") ||
                                    t.MasterLine.NameGroup.Contains("Sub") ||
                                    t.MasterLine.NameGroup.Contains("Sample"))
                                    )
                                .SumAsync(t => (decimal)t.MinuteDay);

                            sewerVal = await Sewer
                                .Where(t =>
                                    t.DateQC == targetDate &&
                                    t.Line.Contains(comp) &&
                                    (
                                    t.MasterLine.NameGroup.Contains("SP1(SAMPLE)") ||
                                    t.MasterLine.NameGroup.Contains("Sub") ||
                                    t.MasterLine.NameGroup.Contains("Sample"))
                                    )
                                .SumAsync(t => (decimal)t.Worker);

                            otVal = await OT
                                .Where(t =>
                                    t.DateQC == targetDate &&
                                    t.Line.Contains(comp) &&
                                    (
                                    t.MasterLine.NameGroup.Contains("SP1(SAMPLE)") ||
                                    t.MasterLine.NameGroup.Contains("Sub") ||
                                    t.MasterLine.NameGroup.Contains("Sample"))
                                    )
                                .SumAsync(t => (decimal)t.WorkerOT);
                        }
                    }

                    var count = results.Count();
                    var sumSam = results.Sum(t => t.Sam);
                    var avgSam = count > 0 ? sumSam / count : 0;

                    decimal minutePercent = minDayVal != 0 ? (decimal)minActVal / (decimal)minDayVal * 100 : 0;

                    decimal totalDefectQc = defectVal + qcVal;
                    decimal defectPercent = totalDefectQc != 0 ? (decimal)defectVal / totalDefectQc * 100 : 0;


                    return Ok(new
                    {
                        results,
                        monitor = new
                        {
                            defectPercent,
                            minDayVal,
                            minActVal,
                            minutePercent,
                            sewerVal,
                            otVal,
                            avgSam
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("GetSupMoniData")]
        public async Task<IActionResult> GetSupData(SupMonitoringRequest request)
        {
            try
            {
                string formattedDate = request.date.ToString("MM/dd/yyyy");
                string comp = "";
                if (request.Company == "YPT") comp = "Y";
                if (request.Company == "GNX") comp = "G";
                var dateParam = new SqlParameter("@date", SqlDbType.NVarChar, 10) { Value = formattedDate };
                var MinuteDay = new SqlParameter("@day", SqlDbType.NVarChar, 20) { Value = "0" };
                var MinuteOT = new SqlParameter("@ot", SqlDbType.NVarChar, 20) { Value = "0" };
                var Fac = new SqlParameter("@fac", SqlDbType.NVarChar, 400) { Value = comp };
                var LineType = new SqlParameter("@line", SqlDbType.SmallInt) { Value = 0 };

                var query = await _db.SupMonitorings
                    .FromSqlRaw("EXEC [Netgarment].dbo.sp_MonitoringLineGroup @date, @day, @ot,@fac,@line", dateParam, MinuteDay, MinuteOT, Fac, LineType)
                    .ToListAsync();

                var results = query;

                if (request.mode == "All")
                {
                    results = query;

                }
                else if (request.mode == "Normal")
                {
                    results = query.Where(t => !t.NameGroup.Contains("SP") && !t.NameGroup.Contains("Supplier") && !t.NameGroup.Contains("Sample")).ToList();

                }
                else // SAMPLE
                {
                    results = query.Where(t => t.NameGroup.Contains("SP1") || t.NameGroup.Contains("Supplier") || t.NameGroup.Contains("Sample")).ToList();

                }

                return Ok(results);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("GetMoniDataDynamic")]
        public async Task<IActionResult> GetDataDynamic()
        {
            try
            {
                using (var connection = new SqlConnection("Data Source=192.168.9.9;Initial Catalog=YPTDB_MAIN;User ID=sa;Password=P@ssw0rd;TrustServerCertificate=True;Persist Security Info=True;"))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("EXEC [Netgarment].dbo.sp_MonitoringLineDetail @date, @flag, @id", connection))
                    {
                        command.Parameters.AddWithValue("@date", "2025-07-18");
                        command.Parameters.AddWithValue("@flag", "Y");
                        command.Parameters.AddWithValue("@id", 0);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var results = new List<Dictionary<string, object>>();

                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var columnName = reader.GetName(i);
                                    var value = reader.GetValue(i);
                                    var dataType = reader.GetFieldType(i);

                                    row[columnName] = new
                                    {
                                        Value = value,
                                        DataType = dataType.Name,
                                        IsDBNull = reader.IsDBNull(i)
                                    };
                                }

                                results.Add(row);
                            }

                            return Ok(results);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("GetMoniDataDynamics")]
        public async Task<IActionResult> GetDataDynamics()
        {
            try
            {
                using (var connection = new SqlConnection("Data Source=192.168.9.9;Initial Catalog=YPTDB_MAIN;User ID=sa;Password=P@ssw0rd;TrustServerCertificate=True;Persist Security Info=True;"))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("EXEC [Netgarment].dbo.sp_MonitoringLineDetail @date, @flag, @id", connection))
                    {
                        command.Parameters.AddWithValue("@date", "07/22/2025");
                        command.Parameters.AddWithValue("@flag", "Y");
                        command.Parameters.AddWithValue("@id", 0);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var mismatchedColumns = new List<object>();

                            // สร้าง Dictionary ของ expected types จาก Monitoring model
                            var expectedTypes = GetMonitoringModelTypes();

                            // ตรวจสอบ column แรกเพื่อเปรียบเทียบ type
                            if (await reader.ReadAsync())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var columnName = reader.GetName(i);
                                    var actualType = reader.GetFieldType(i);

                                    if (expectedTypes.ContainsKey(columnName))
                                    {
                                        var expectedType = expectedTypes[columnName];
                                        var actualTypeName = GetCSharpTypeName(actualType);

                                        if (expectedType != actualTypeName)
                                        {
                                            mismatchedColumns.Add(new
                                            {
                                                ColumnName = columnName,
                                                ExpectedType = expectedType,
                                                ActualType = actualTypeName,
                                                DatabaseType = actualType.Name,
                                                SampleValue = reader.IsDBNull(i) ? null : reader.GetValue(i),
                                                SuggestedFix = GetSuggestedFix(expectedType, actualTypeName, columnName)
                                            });
                                        }
                                    }
                                }
                            }

                            return Ok(new
                            {
                                MismatchedColumns = mismatchedColumns,
                                TotalColumns = reader.FieldCount,
                                MismatchCount = mismatchedColumns.Count
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSupMoniType")]
        public async Task<IActionResult> GetDataTypeDynamics()
        {
            try
            {
                using (var connection = new SqlConnection("Data Source=192.168.9.9;Initial Catalog=YPTDB_MAIN;User ID=sa;Password=P@ssw0rd;TrustServerCertificate=True;Persist Security Info=True;"))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("EXEC [Netgarment].dbo.sp_MonitoringLineGroup @date, @day, @ot,@fac,@line", connection))
                    {
                        command.Parameters.AddWithValue("@date", "2025-07-18");
                        command.Parameters.AddWithValue("@day", "0");
                        command.Parameters.AddWithValue("@ot", "0");
                        command.Parameters.AddWithValue("@fac", "Y");
                        command.Parameters.AddWithValue("@line", 1);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var mismatchedColumns = new List<object>();

                            // สร้าง Dictionary ของ expected types จาก Monitoring model
                            var expectedTypes = GetSupMonitoringModelTypes();

                            // ตรวจสอบ column แรกเพื่อเปรียบเทียบ type
                            if (await reader.ReadAsync())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var columnName = reader.GetName(i);
                                    var actualType = reader.GetFieldType(i);

                                    if (expectedTypes.ContainsKey(columnName))
                                    {
                                        var expectedType = expectedTypes[columnName];
                                        var actualTypeName = GetCSharpTypeName(actualType);

                                        if (expectedType != actualTypeName)
                                        {
                                            mismatchedColumns.Add(new
                                            {
                                                ColumnName = columnName,
                                                ExpectedType = expectedType,
                                                ActualType = actualTypeName,
                                                DatabaseType = actualType.Name,
                                                SampleValue = reader.IsDBNull(i) ? null : reader.GetValue(i),
                                                SuggestedFix = GetSuggestedFix(expectedType, actualTypeName, columnName)
                                            });
                                        }
                                    }
                                }
                            }

                            return Ok(new
                            {
                                MismatchedColumns = mismatchedColumns,
                                TotalColumns = reader.FieldCount,
                                MismatchCount = mismatchedColumns.Count
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private Dictionary<string, string> GetMonitoringModelTypes()
        {
            var modelTypes = new Dictionary<string, string>();
            var properties = typeof(Monitoring).GetProperties();

            foreach (var prop in properties)
            {
                modelTypes[prop.Name] = GetCSharpTypeName(prop.PropertyType);
            }

            return modelTypes;
        }

        private Dictionary<string, string> GetSupMonitoringModelTypes()
        {
            var modelTypes = new Dictionary<string, string>();
            var properties = typeof(SupMonitoring).GetProperties();

            foreach (var prop in properties)
            {
                modelTypes[prop.Name] = GetCSharpTypeName(prop.PropertyType);
            }

            return modelTypes;
        }

        private string GetCSharpTypeName(Type type)
        {
            // Handle nullable types
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);
            }

            // Map common types
            return type.Name switch
            {
                "Int32" => "int",
                "Int64" => "long",
                "Int16" => "short",
                "Double" => "double",
                "Single" => "float",
                "Decimal" => "decimal",
                "Boolean" => "bool",
                "String" => "string",
                "DateTime" => "DateTime",
                "Guid" => "Guid",
                "Byte" => "byte",
                _ => type.Name
            };
        }

        private string GetSuggestedFix(string expectedType, string actualType, string columnName)
        {
            if (expectedType == "double" && actualType == "decimal")
            {
                return $"Change '{columnName}' from 'public double {columnName}' to 'public decimal {columnName}'";
            }
            else if (expectedType == "int" && actualType == "decimal")
            {
                return $"Change '{columnName}' from 'public int {columnName}' to 'public decimal {columnName}'";
            }
            else if (expectedType == "string" && actualType != "string")
            {
                return $"Change '{columnName}' from 'public string {columnName}' to 'public {actualType} {columnName}'";
            }
            else
            {
                return $"Change '{columnName}' from 'public {expectedType} {columnName}' to 'public {actualType} {columnName}'";
            }
        }

        //public ActionResult GetMatrPlanningNo(List<string> supp, string planningNo)
        //{


        //    var result = new List<MatrInPlanningNoModel>();

        //    using (SqlConnection conn = new SqlConnection("Data Source=192.168.9.9;Initial Catalog=YPTDB_MAIN;User ID=sa;Password=P@ssw0rd;TrustServerCertificate=True;Persist Security Info=True;"))
        //    {
        //        try
        //        {
        //            conn.Open();

        //            using (SqlCommand cmd = new SqlCommand("sp_Generate_PlanningSearch", conn))
        //            {


        //                string supps = string.Join(",", supp); // <- สำคัญ


        //                cmd.CommandType = CommandType.StoredProcedure;
        //                cmd.Parameters.AddWithValue("@SupCode", supps);
        //                cmd.Parameters.AddWithValue("@PlanningNo", planningNo);
        //                cmd.Parameters.AddWithValue("@Factory", "YPT");


        //                decimal TempPurQty = 0;
        //                using (SqlDataReader reader = cmd.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        result.Add(new MatrInPlanningNoModel
        //                        {

        //                            SupplierCode = reader["SupplierCode"] == DBNull.Value ? "" : reader["SupplierCode"].ToString(),
        //                            SupplierName = reader["SupplierName"] == DBNull.Value ? "" : reader["SupplierName"].ToString(),
        //                            MatrClass = reader["MatrClass"] == DBNull.Value ? "" : reader["MatrClass"].ToString(),
        //                            MatrCode = reader["MatrCode"] == DBNull.Value ? "" : reader["MatrCode"].ToString(),
        //                            Color = reader["Color"] == DBNull.Value ? "" : reader["Color"].ToString(),
        //                            Size = reader["Size"] == DBNull.Value ? "" : reader["Size"].ToString(),
        //                            QtyUnit = reader["QtyUnit"] == DBNull.Value ? "" : reader["QtyUnit"].ToString(),
        //                            Currency = reader["Currency"] == DBNull.Value ? "" : reader["Currency"].ToString(),
        //                            MasterPrice = reader["MasterPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["MasterPrice"]),
        //                            ReqQty = reader["ReqQty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ReqQty"]),
        //                            PercentLoss = reader["PercentLoss"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["PercentLoss"]),
        //                            AddMatrLoss = reader["AddMatrLoss"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AddMatrLoss"]),
        //                            StkQty = reader["StkQty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["StkQty"]),

        //                            //PurQty = reader["PurQty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["PurQty"]),

        //                            PurQty =
        //                                    ((reader["ReqQty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ReqQty"])) +
        //                                    (reader["AddMatrLoss"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AddMatrLoss"]))),



        //                            PONo = reader["PONo"] == DBNull.Value ? "" : reader["PONo"].ToString(),

        //                            Description = reader["Description"] == DBNull.Value ? "" : reader["Description"].ToString(),
        //                            Specification = reader["ShortName"] == DBNull.Value ? "" : reader["ShortName"].ToString(),


        //                            StkYPT = 0,
        //                            FreeQty = 0,
        //                            PoQty = ((reader["ReqQty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ReqQty"])) +
        //                                    (reader["AddMatrLoss"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AddMatrLoss"]))),
        //                            RatioFB = 0

        //                        });
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("ERROR: " + ex.Message);
        //        }
        //    }

        //    return new JsonNetResult(result);


        //}
    }
}
