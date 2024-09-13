using Microsoft.AspNetCore.Mvc;
using IbeApi.Models;
using Rotativa.AspNetCore;
using System.Data.SqlClient;

namespace IbeApi.Controllers
{
    public class ReportPageController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<CandidatoController> _logger;
        public ReportPageController(IConfiguration configuration, ILogger<CandidatoController> logger) {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger;
        }


        public IActionResult reportPage(String email)
        {
            _logger.LogInformation("Get request received for candidate ID {codcandi}", email);

            CandidatoDTO candidato = new CandidatoDTO();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.LogInformation("Database connection opened.");

                    //const string sql = "SELECT CODCANDI, GBICANDI.NOME, ESPECIAL, GBIEDITA.NOME AS EDITAL, GBICANDI.CODEDITA, DATADENA, DATAEMIS, VALIDO, GBICANDI.IDADE, APELIDO, NUMEO, NOMECOMP, EMAIL, TELEFONE, TELEMOVE, GENERO, ESTADODO, OCUPACAO, NATURALI, RUA, GBIPROVI.CODPROVI, GBIPROVI.PROVINCI, AREAS, GBICANDI.CODAREA FROM GBICANDI JOIN GBIPROVI ON GBIPROVI.CODPROVI = GBICANDI.CODPROVI JOIN GBIEDITA ON GBIEDITA.CODEDITA = GBICANDI.CODEDITA JOIN GBIAREA ON GBIAREA.CODAREA = GBICANDI.CODAREA WHERE EMAIL = @EMAIL AND PASSWORD = @PASSWORD";
                    const string sql = @"
                        SELECT 
                            CODCANDI, 
                            GBICANDI.NOME, 
                            CASE 
                                WHEN ESTADODO = 'P' THEN 'Em avaliação'
                                WHEN ESTADODO = 'R' THEN 'Reprovado'
                                WHEN ESTADODO = 'A' THEN 'Aprovado'
                                ELSE 'UNKNOWN'
                            END AS ESTADODO,
                            ESPECIAL, 
                            GBIEDITA.NOME AS EDITAL, 
                            GBICANDI.CODEDITA, 
                            DATADENA, 
                            DATAEMIS, 
                            VALIDO, 
                            GBICANDI.IDADE, 
                            APELIDO, 
                            NUMEO, 
                            NOMECOMP, 
                            EMAIL, 
                            TELEFONE, 
                            TELEMOVE, 
                            GENERO, 
                            PONTUACA,
                            GBICANDI.NIVEL,
                            CASE
                                WHEN GBICANDI.NIVEL = 'E' THEN 'Médio'
                                WHEN GBICANDI.NIVEL = 'L' THEN 'Licenciatura'
                                WHEN GBICANDI.NIVEL = 'P' THEN 'ETP'
                                WHEN GBICANDI.NIVEL = 'M' THEN 'Mestrado'
                                WHEN GBICANDI.NIVEL = 'D' THEN 'Dutoramento'
                                ELSE 'UNKNOWN'
                            END AS NIVEL_DESCRICAO,
                            OCUPACAO, 
                            NATURALI, 
                            RUA, 
                            GBIPROVI.CODPROVI, 
                            GBIPROVI.PROVINCI, 
                            AREAS, 
                            GBICANDI.CODAREA 
                        FROM 
                            GBICANDI 
                        JOIN 
                            GBIPROVI ON GBIPROVI.CODPROVI = GBICANDI.CODPROVI 
                        JOIN 
                            GBIEDITA ON GBIEDITA.CODEDITA = GBICANDI.CODEDITA 
                        JOIN 
                            GBIAREA ON GBIAREA.CODAREA = GBICANDI.CODAREA 
                        WHERE 
                            EMAIL = @EMAIL";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@EMAIL", email);

                        _logger.LogInformation("SQL command prepared: {sql}", sql);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                candidato.codcandi = reader.GetInt32(reader.GetOrdinal("CODCANDI"));
                                candidato.nome = reader.IsDBNull(reader.GetOrdinal("NOME")) ? null : reader.GetString(reader.GetOrdinal("NOME"));
                                candidato.apelido = reader.IsDBNull(reader.GetOrdinal("APELIDO")) ? null : reader.GetString(reader.GetOrdinal("APELIDO"));
                                candidato.nomecomp = reader.IsDBNull(reader.GetOrdinal("NOMECOMP")) ? null : reader.GetString(reader.GetOrdinal("NOMECOMP"));
                                candidato.email = reader.IsDBNull(reader.GetOrdinal("EMAIL")) ? null : reader.GetString(reader.GetOrdinal("EMAIL"));
                                candidato.telefone = reader.IsDBNull(reader.GetOrdinal("TELEFONE")) ? null : reader.GetString(reader.GetOrdinal("TELEFONE"));
                                candidato.telemovel = reader.IsDBNull(reader.GetOrdinal("TELEMOVE")) ? null : reader.GetString(reader.GetOrdinal("TELEMOVE"));
                                candidato.identificacao = reader.IsDBNull(reader.GetOrdinal("NUMEO")) ? null : reader.GetString(reader.GetOrdinal("NUMEO"));
                                candidato.genero = reader.IsDBNull(reader.GetOrdinal("GENERO")) ? null : reader.GetString(reader.GetOrdinal("GENERO"));
                                candidato.idade = reader.IsDBNull(reader.GetOrdinal("IDADE")) ? 0 : reader.GetInt16(reader.GetOrdinal("IDADE"));
                                candidato.estado = reader.IsDBNull(reader.GetOrdinal("ESTADODO")) ? null : reader.GetString(reader.GetOrdinal("ESTADODO"));
                                candidato.ocupacao = reader.IsDBNull(reader.GetOrdinal("OCUPACAO")) ? null : reader.GetString(reader.GetOrdinal("OCUPACAO"));
                                candidato.naturalidade = reader.IsDBNull(reader.GetOrdinal("NATURALI")) ? null : reader.GetString(reader.GetOrdinal("NATURALI"));
                                candidato.rua = reader.IsDBNull(reader.GetOrdinal("RUA")) ? null : reader.GetString(reader.GetOrdinal("RUA"));
                                //candidato.datadena = reader.GetDateTime("DATADENA");
                                //candidato.data_emissao = reader.GetDateTime("DATAEMIS");
                                //candidato.data_validade = reader.GetDateTime("VALIDO");
                                candidato.provincia = reader.IsDBNull(reader.GetOrdinal("PROVINCI")) ? null : reader.GetString(reader.GetOrdinal("PROVINCI"));
                                candidato.codprovi = reader.GetInt32(reader.GetOrdinal("CODPROVI"));
                                candidato.edital = reader.IsDBNull(reader.GetOrdinal("EDITAL")) ? null : reader.GetString(reader.GetOrdinal("EDITAL"));
                                candidato.especialidade = reader.IsDBNull(reader.GetOrdinal("ESPECIAL")) ? null : reader.GetString(reader.GetOrdinal("ESPECIAL"));
                                candidato.area = reader.IsDBNull(reader.GetOrdinal("AREAS")) ? null : reader.GetString(reader.GetOrdinal("AREAS"));//NIVEL_DESCRICAO
                                candidato.nivel = reader.IsDBNull(reader.GetOrdinal("NIVEL_DESCRICAO")) ? null : reader.GetString(reader.GetOrdinal("NIVEL_DESCRICAO"));                                                                                          //candidato.codarea = reader.GetInt32(reader.GetOrdinal("CODAREA"));
                                candidato.pontuacao = reader.IsDBNull(reader.GetOrdinal("PONTUACA")) ? 0 : reader.GetInt16(reader.GetOrdinal("PONTUACA"));
                                candidato.FindTrue = true; // Set to true if candidate is found


                                _logger.LogInformation("Candidate data retrieved successfully for {email}", email);
                            }
                            else
                            {
                                _logger.LogWarning("Candidate with {email} not found.", email);
                                return NotFound(candidato);
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error occurred while fetching candidate data for ID {codcandi}", email);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching candidate data" + sqlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching candidate data for ID {codcandi}", email);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred" + ex);
            }

            // return Ok(candidato);
            return View("report_page", candidato);
        }
    }
}
