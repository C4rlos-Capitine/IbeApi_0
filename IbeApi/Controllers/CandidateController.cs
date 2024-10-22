using IbeApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly string _connectionString;

        public CandidateController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] Candidate candidate)
        {
            int cod_zona = getProvincZone(candidate.codprovi);
            if (candidate == null)
            {
                return BadRequest("Candidate data is null");
            }

            // Validate if files are present
            if (candidate.bi_file == null || candidate.nuit_file == null || candidate.certificado_file == null || candidate.foto_passe == null)
            {
                return BadRequest("Please upload all required files.");
            }

            // Check if identification number or email already exists
            if (IsCandidateExists(candidate.num_ident, candidate.email))
            {
                return Ok(new { Message = "BI ou email já existente.", Code = candidate.codcandi, success = false });
            }

            int codcandi = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Insert candidate data into the database
                    const string sql = @"
                    INSERT INTO GBICANDI (CODCANDI, CODPROVI, PASSWORD, NOME, APELIDO, NOMECOMP, NUMEO, EMAIL, TELEFONE, TELEMOVE, GENERO, DATADENA, IDADE, OCUPACAO, NATURALI, RUA, DATAEMIS, VALIDO, CODEDITA, CODAREA, NVEL, NIVEL, ESPECIAL, ESTADODO, TIPODEDO, DATASUBM, CANDIDA, BI, NUIT, CERTIFICADO, FOTOPASSE, NUIT, MEDIAOBT)
                    OUTPUT INSERTED.CODCANDI
                    VALUES (@CODCANDI, @CODPROVI, @PASSWORD, @NOME, @APELIDO, @NOMECOMP, @NUMEO, @EMAIL, @TELEFONE, @TELEMOVE, @GENERO, @DATADENA, @IDADE, @OCUPACAO, @NATURALI, @RUA, @DATAEMIS, @VALIDO, @CODEDITA, @CODAREA, @NVEL, @NIVEL, @ESPECIAL, @ESTADODO, @TIPODEDO, @DATASUBM, @CANDIDA, @BI, @NUIT, @CERTIFICADO, @FOTOPASSE, @NUIT, @MEDIAOBT);";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@CODCANDI", codcandi);
                        command.Parameters.AddWithValue("@CODPROVI", candidate.codprovi);

                        command.Parameters.AddWithValue("@NOME", (object)candidate.nome ?? DBNull.Value);
                        command.Parameters.AddWithValue("@APELIDO", (object)candidate.apelido ?? DBNull.Value);
                        command.Parameters.AddWithValue("@PASSWORD", (object)candidate.password ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NOMECOMP", (object)candidate.nomecomp ?? DBNull.Value);
                        command.Parameters.AddWithValue("@EMAIL", (object)candidate.email ?? DBNull.Value);
                        command.Parameters.AddWithValue("@TELEFONE", (object)candidate.telefone ?? DBNull.Value);
                        command.Parameters.AddWithValue("@TELEMOVE", (object)candidate.telemovel ?? DBNull.Value);
                        command.Parameters.AddWithValue("@GENERO", (object)candidate.genero ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DATADENA", new DateTime(candidate.ano, candidate.mes, candidate.dia));
                        command.Parameters.AddWithValue("@DATAEMIS", new DateTime(candidate.ano_emissao, candidate.mes_emissao, candidate.dia_emissao));
                        command.Parameters.AddWithValue("@VALIDO", new DateTime(candidate.ano_validade, candidate.mes_validade, candidate.dia_validade));
                        command.Parameters.AddWithValue("@NUMEO", (object)candidate.num_ident ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IDADE", (object)candidate.idade ?? DBNull.Value);
                        command.Parameters.AddWithValue("@OCUPACAO", (object)candidate.ocupacao ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NATURALI", (object)candidate.naturalidade ?? DBNull.Value);
                        command.Parameters.AddWithValue("@RUA", (object)candidate.rua ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CODEDITA", (object)candidate.codedital ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CODAREA", (object)candidate.codarea ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ESPECIAL", (object)candidate.especialidade ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NVEL", (object)candidate.nivel ?? DBNull.Value); //ESTADODO
                        command.Parameters.AddWithValue("@NIVEL", (object)candidate.nivel ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ESTADODO", "P");
                        command.Parameters.AddWithValue("@TIPODEDO", (object)candidate.tipo_doc ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DATASUBM", DateTime.Now);
                        command.Parameters.AddWithValue("@CANDIDA", 1);
                        command.Parameters.AddWithValue("@CODZONA", cod_zona);
                        command.Parameters.AddWithValue("@NUIT", (object)candidate.nuit ?? DBNull.Value);//MEDIAOBT
                        command.Parameters.AddWithValue("@MEDIAOBT", (object)candidate.media_obt ?? DBNull.Value);
                        // Convert files to byte arrays and add to the parameters
                        command.Parameters.Add("@BI", SqlDbType.VarBinary).Value = await GetFileBytes(candidate.bi_file);
                        command.Parameters.Add("@NUIT", SqlDbType.VarBinary).Value = await GetFileBytes(candidate.nuit_file);
                        command.Parameters.Add("@CERTIFICADO", SqlDbType.VarBinary).Value = await GetFileBytes(candidate.certificado_file);
                        command.Parameters.Add("@FOTOPASSE", SqlDbType.VarBinary).Value = await GetFileBytes(candidate.foto_passe);

                        // Execute and get the candidate ID
                        candidate.codcandi = (int)await command.ExecuteScalarAsync();
                    }
                }

                return Ok(new { Message = "Candidate registered successfully", Code = candidate.codcandi, success = true });
            }
            catch (SqlException sqlEx)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"SQL Error: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Error: {ex.Message}");
            }
        }

        private async Task<byte[]> GetFileBytes(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private bool IsCandidateExists(string numIdent, string email)
        {
            // Logic to check if the candidate exists in the database
            return false; // Replace with actual logic
        }
        private int getProvincZone(int codProv)
        {

            using (var connection = new SqlConnection(_connectionString))
            {
                int zona;
                connection.Open();
                const string sql = @"
            SELECT CODZONA
            FROM GBIPROVI
            WHERE CODPROVI = @CODPROVI;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CODPROVI", codProv);

                    //var reader = command.ExecuteReader()
                    zona = (int)command.ExecuteScalar();

                    return zona;
                }
            }

        }
    }
}
