using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IbeApi.Models;
using System.Data.SqlClient;
using System.Data;
using IbeApi.Services;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatoController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<CandidatoController> _logger;

        public CandidatoController(IConfiguration configuration, ILogger<CandidatoController> logger)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger;
        }

        [HttpGet("{codcandi}")]
        public IActionResult Get(int codcandi)
        {
            _logger.LogInformation("Get request received for candidate ID {codcandi}", codcandi);

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
                            DATASUBM,
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
                            GBICANDI.NVEL,
                            CASE
                                WHEN GBICANDI.NVEL = 'E' THEN 'Médio'
                                WHEN GBICANDI.NVEL = 'L' THEN 'Licenciatura'
                                WHEN GBICANDI.NVEL = 'ET' THEN 'ETP'
                                WHEN GBICANDI.NVEL = 'M' THEN 'Mestrado'
                                WHEN GBICANDI.NVEL = 'D' THEN 'Dutoramento'
                            END AS NIVEL_DESCRICAO,
                            OCUPACAO, 
                            NATURALI, 
                            RUA, 
                            GBIPROVI.CODPROVI, 
                            GBIPROVI.PROVINCI, 
                            AREAS, 
	                        GBIDISTR.DISTRITO,
	                        GBICANDI.BAIRRO,
                            GBICANDI.CODAREA,
                            GBIEDITA.TIPOSBOL, 
                            CASE
		                         WHEN GBIEDITA.TIPOSBOL = 'I' THEN 'Bolsa interna'
		                         WHEN GBIEDITA.TIPOSBOL = 'E' THEN 'Bolsa externa'
	                        END AS TIPO_BOLSA
                        FROM 
                            GBICANDI 
                        JOIN 
                            GBIPROVI ON GBIPROVI.CODPROVI = GBICANDI.CODPROVI 
                        JOIN 
                            GBIEDITA ON GBIEDITA.CODEDITA = GBICANDI.CODEDITA 
                        JOIN 
                            GBIAREA ON GBIAREA.CODAREA = GBICANDI.CODAREA 
                        JOIN 
	                        GBIDISTR ON GBIDISTR.CODDISTR = GBICANDI.CODDISTR
                        WHERE 
                            CODCANDI = @CODCANDI";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@CODCANDI", codcandi);
                    
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
                                candidato.datadena = reader.GetDateTime("DATADENA");
                                candidato.data_emissao = reader.GetDateTime("DATAEMIS");
                                candidato.data_subm = reader.GetDateTime("DATASUBM");
                                candidato.data_validade = reader.GetDateTime("VALIDO");
                                candidato.provincia = reader.IsDBNull(reader.GetOrdinal("PROVINCI")) ? null : reader.GetString(reader.GetOrdinal("PROVINCI"));
                                candidato.codprovi = reader.GetInt32(reader.GetOrdinal("CODPROVI"));
                                candidato.edital = reader.IsDBNull(reader.GetOrdinal("EDITAL")) ? null : reader.GetString(reader.GetOrdinal("EDITAL"));
                                candidato.especialidade = reader.IsDBNull(reader.GetOrdinal("ESPECIAL")) ? null : reader.GetString(reader.GetOrdinal("ESPECIAL"));
                                candidato.area = reader.IsDBNull(reader.GetOrdinal("AREAS")) ? null : reader.GetString(reader.GetOrdinal("AREAS"));//NIVEL_DESCRICAO
                                candidato.nivel = reader.IsDBNull(reader.GetOrdinal("NIVEL_DESCRICAO")) ? null : reader.GetString(reader.GetOrdinal("NIVEL_DESCRICAO"));                                                                                          //candidato.codarea = reader.GetInt32(reader.GetOrdinal("CODAREA"));
                                candidato.pontuacao = reader.IsDBNull(reader.GetOrdinal("PONTUACA")) ? 0 : reader.GetInt16(reader.GetOrdinal("PONTUACA"));
                                candidato.tipo_bolsa = reader.IsDBNull(reader.GetOrdinal("TIPO_BOLSA")) ? null : reader.GetString(reader.GetOrdinal("TIPO_BOLSA"));
                                candidato.FindTrue = true; // Set to true if candidate is found


                                _logger.LogInformation("Candidate data retrieved successfully for {codcandi}", codcandi);
                            }
                            else
                            {
                                _logger.LogWarning("Candidate with {codcandi} not found.", codcandi);
                                return NotFound(candidato);
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error occurred while fetching candidate data for ID {codcandi}", codcandi);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching candidate data"+sqlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching candidate data for ID {codcandi}", codcandi);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred"+ex);
            }

            return Ok(candidato);
        }

        [HttpGet("search")]
        public IActionResult GetByEmailAndPhone([FromQuery] string email, [FromQuery] string password)
        {
            _logger.LogInformation("Get request received for candidate with email {email} and phone {telefone}", email, password);

            CandidatoDTO candidato = new CandidatoDTO
            {
                FindTrue = false // Default to false
            };

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
                            DATASUBM,
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
                            GBICANDI.NVEL,
                            CASE
                                WHEN GBICANDI.NVEL = 'E' THEN 'Médio'
                                WHEN GBICANDI.NVEL = 'L' THEN 'Licenciatura'
                                WHEN GBICANDI.NVEL = 'ET' THEN 'ETP'
                                WHEN GBICANDI.NVEL = 'M' THEN 'Mestrado'
                                WHEN GBICANDI.NVEL = 'D' THEN 'Dutoramento'
                            END AS NIVEL_DESCRICAO,
                            OCUPACAO, 
                            NATURALI, 
                            RUA, 
                            GBIPROVI.CODPROVI, 
                            GBIPROVI.PROVINCI, 
                            AREAS, 
	                        GBIDISTR.DISTRITO,
	                        GBICANDI.BAIRRO,
                            GBICANDI.CODAREA,
                            GBIEDITA.TIPOSBOL, 
                            CASE
		                         WHEN GBIEDITA.TIPOSBOL = 'I' THEN 'Bolsa interna'
		                         WHEN GBIEDITA.TIPOSBOL = 'E' THEN 'Bolsa externa'
	                        END AS TIPO_BOLSA
                        FROM 
                            GBICANDI 
                        JOIN 
                            GBIPROVI ON GBIPROVI.CODPROVI = GBICANDI.CODPROVI 
                        JOIN 
                            GBIEDITA ON GBIEDITA.CODEDITA = GBICANDI.CODEDITA 
                        JOIN 
                            GBIAREA ON GBIAREA.CODAREA = GBICANDI.CODAREA 
                        JOIN 
	                        GBIDISTR ON GBIDISTR.CODDISTR = GBICANDI.CODDISTR
                        WHERE 
                            EMAIL = @EMAIL AND PASSWORD = @PASSWORD";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@EMAIL", email);
                        command.Parameters.AddWithValue("@PASSWORD", password);
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
                                candidato.datadena = reader.GetDateTime("DATADENA");
                                candidato.data_emissao = reader.GetDateTime("DATAEMIS");
                                candidato.data_subm = reader.GetDateTime("DATASUBM");
                                candidato.data_validade = reader.GetDateTime("VALIDO");
                                candidato.provincia = reader.IsDBNull(reader.GetOrdinal("PROVINCI")) ? null : reader.GetString(reader.GetOrdinal("PROVINCI"));
                                candidato.codprovi = reader.GetInt32(reader.GetOrdinal("CODPROVI"));
                                candidato.edital = reader.IsDBNull(reader.GetOrdinal("EDITAL")) ? null : reader.GetString(reader.GetOrdinal("EDITAL"));
                                candidato.especialidade = reader.IsDBNull(reader.GetOrdinal("ESPECIAL")) ? null : reader.GetString(reader.GetOrdinal("ESPECIAL"));
                                candidato.area = reader.IsDBNull(reader.GetOrdinal("AREAS")) ? null : reader.GetString(reader.GetOrdinal("AREAS"));//NIVEL_DESCRICAO
                                candidato.nivel = reader.IsDBNull(reader.GetOrdinal("NIVEL_DESCRICAO")) ? null : reader.GetString(reader.GetOrdinal("NIVEL_DESCRICAO"));                                                                                          //candidato.codarea = reader.GetInt32(reader.GetOrdinal("CODAREA"));
                                candidato.pontuacao = reader.IsDBNull(reader.GetOrdinal("PONTUACA")) ? 0 : reader.GetInt16(reader.GetOrdinal("PONTUACA"));
                                candidato.tipo_bolsa = reader.IsDBNull(reader.GetOrdinal("TIPO_BOLSA")) ? null : reader.GetString(reader.GetOrdinal("TIPO_BOLSA")); 
                                candidato.FindTrue = true; // Set to true if candidate is found


                                _logger.LogInformation("Candidate data retrieved successfully for email {email} and phone {telefone}", email, password);
                            }
                            else
                            {
                                _logger.LogWarning("Candidate with email {email} and phone {telefone} not found.", email, password);
                                return NotFound(candidato);
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error occurred while fetching candidate data for email {email} and phone {telefone}", email, password);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching candidate data: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching candidate data for email {email} and phone {telefone}", email, password);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred: " + ex.Message);
            }

            return Ok(candidato);
        }



        [HttpPost]
        public IActionResult Post([FromBody] Candidato candidato)
        {

            int cod_zona = getProvincZone(candidato.codprovi);

            if (candidato == null)
            {
                _logger.LogWarning("Post request received with null candidate.");
                return BadRequest("Candidate data is null");
            }

            // Verificar se o NUMEO ou EMAIL já existem
            if (IsCandidatoExists(candidato.num_ident, candidato.email))
            {
                _logger.LogWarning(" NUMEO {numeo} or EMAIL {email} already exists.", candidato.num_ident, candidato.email);
                //return Conflict("BI ou email já existente.");
                var result = new
                {
                    Message = "BI ou email já existente.",
                    Code = candidato.codcandi,
                    success = false,
                };
                return Ok(result);
            }

            //int[] dadosLocalidade = GetInfoByLocalidade(candidato.posto);

            //int codcandi = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);
            //int codcandi = getLastCodCandi();
          
            _logger.LogInformation("Post request received for candidate: {candidato}");

            try
            {
                  int codcandi = codProximoCandidato();
                setCodProximoCandi(codcandi);
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.LogInformation("Database connection opened.");



                    const string sql = @"
                INSERT INTO GBICANDI (CODCANDI, CODPROVI, PASSWORD, NOME, APELIDO, NOMECOMP, NUMEO, EMAIL, TELEFONE, TELEMOVE, GENERO, DATADENA, IDADE, OCUPACAO, NATURALI, RUA, DATAEMIS, VALIDO, CODEDITA, CODAREA, NVEL, NIVEL, ESPECIAL, ESTADODO, TIPODEDO, DATASUBM, CODZONA, CANDIDA, NUIT, MEDIAOBT, EORFAO, PAI, MAE, CODPOSTO, CODDISTR, BAIRRO)
                OUTPUT INSERTED.CODCANDI
                VALUES (@CODCANDI, @CODPROVI, @PASSWORD, @NOME, @APELIDO, @NOMECOMP, @NUMEO, @EMAIL, @TELEFONE, @TELEMOVE, @GENERO, @DATADENA, @IDADE, @OCUPACAO, @NATURALI, @RUA, @DATAEMIS, @VALIDO, @CODEDITA, @CODAREA, @NVEL, @NIVEL, @ESPECIAL, @ESTADODO, @TIPODEDO, @DATASUBM, @CODZONA, @CANDIDA, @NUIT, @MEDIAOBT, @EORFAO, @PAI, @MAE, @CODPOSTO, @CODDISTR, @BAIRRO);";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@CODCANDI", codcandi);
                        command.Parameters.AddWithValue("@CODPROVI", (object)candidato.codprovi ?? DBNull.Value);
                        
                        command.Parameters.AddWithValue("@NOME", (object)candidato.nome ?? DBNull.Value);
                        command.Parameters.AddWithValue("@APELIDO", (object)candidato.apelido ?? DBNull.Value);
                        command.Parameters.AddWithValue("@PASSWORD", (object)candidato.password ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NOMECOMP", (object)candidato.nomecomp ?? DBNull.Value);
                        command.Parameters.AddWithValue("@EMAIL", (object)candidato.email ?? DBNull.Value);
                        command.Parameters.AddWithValue("@TELEFONE", (object)candidato.telefone ?? DBNull.Value);
                        command.Parameters.AddWithValue("@TELEMOVE", (object)candidato.telemovel ?? DBNull.Value);
                        command.Parameters.AddWithValue("@GENERO", (object)candidato.genero ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DATADENA", new DateTime(candidato.ano, candidato.mes, candidato.dia));
                        command.Parameters.AddWithValue("@DATAEMIS", new DateTime(candidato.ano_emissao, candidato.mes_emissao, candidato.dia_emissao));
                        command.Parameters.AddWithValue("@VALIDO", new DateTime(candidato.ano_validade, candidato.mes_validade, candidato.dia_validade));
                        command.Parameters.AddWithValue("@NUMEO", (object)candidato.num_ident ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IDADE", (object)candidato.idade ?? DBNull.Value);
                        command.Parameters.AddWithValue("@OCUPACAO", (object)candidato.ocupacao ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NATURALI", (object)candidato.naturalidade ?? DBNull.Value);
                        command.Parameters.AddWithValue("@RUA", (object)candidato.rua ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CODEDITA", (object)candidato.codedital ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CODAREA", (object)candidato.codarea ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ESPECIAL", (object)candidato.especialidade ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NVEL", (object)candidato.nivel ?? DBNull.Value);//ESTADODO
                        command.Parameters.AddWithValue("@NIVEL", (object)candidato.nivel ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ESTADODO", "P");
                        command.Parameters.AddWithValue("@TIPODEDO", (object)candidato.tipo_doc ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DATASUBM", DateTime.Now);
                        command.Parameters.AddWithValue("@CANDIDA", 1);
                        command.Parameters.AddWithValue("@CODZONA", cod_zona);
                        command.Parameters.AddWithValue("@NUIT", (object)candidato.nuit ?? DBNull.Value);//MEDIAOBT
                        command.Parameters.AddWithValue("@MEDIAOBT", (object)candidato.media_obt ?? DBNull.Value);
                        command.Parameters.AddWithValue("@EORFAO", (object)candidato.eorfao ?? DBNull.Value);
                        command.Parameters.AddWithValue("@PAI", (object)candidato.pai ?? DBNull.Value);
                        command.Parameters.AddWithValue("@MAE", (object)candidato.mae ?? DBNull.Value);//CODDISTR
                        command.Parameters.AddWithValue("@CODPOSTO", (object)candidato.distrito);
                        command.Parameters.AddWithValue("@CODDISTR", candidato.distrito);//BAIRRO
                        command.Parameters.AddWithValue("@BAIRRO", (object)candidato.bairro ?? DBNull.Value);

                        // Executar o comando e obter o ID do candidato inserido
                        candidato.codcandi = (int)command.ExecuteScalar();
                        _logger.LogInformation("Candidate data inserted successfully with ID {codcandi}", candidato.codcandi);

                    }
                }


                CreateMsg(candidato.email, candidato.codedital);
                regFiliacao(codcandi + 1, candidato.nomepai, BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0), "P", candidato.filho_combatente);
                regFiliacao(codcandi + 1, candidato.nomepai, BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0), "M", candidato.filho_combatente);

                var result = new
                {
                    Message = "CandidatO registadO Com sucesso",
                    Code = candidato.codcandi,
                    success = true,
                    //mailData = mail_service
                };

                return Ok(result);
               // return CreatedAtAction(nameof(Get), new { codcandi = candidato.codcandi }, result);
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error occurred while inserting candidate data.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while inserting candidate data" + sqlEx);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting candidate data.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred" + ex);
            }
        }

        private bool IsCandidatoExists(String numeo, string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                const string sql = @"
            SELECT COUNT(*)
            FROM GBICANDI
            WHERE NUMEO = @NUMEO OR EMAIL = @EMAIL;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@NUMEO", (object)numeo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EMAIL", (object)email ?? DBNull.Value);

                    var count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
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
        private int codProximoCandidato()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                const string sql = @"
        SELECT proximo
        FROM Codigos_Sequenciais
        WHERE id_objecto = @id_objecto;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id_objecto", "GBICANDI");

                    // Retrieve as long and cast to int if it fits within range
                   int  cod = (int)(long)command.ExecuteScalar();

                    return cod;
                }
            }
        }

        private void setCodProximoCandi(int cod)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                const string sql = @"
                    UPDATE Codigos_Sequenciais
                    SET proximo = @proximo
                    WHERE id_objecto = @id_objecto;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@proximo", cod + 1); // Increment cod by 1
                    command.Parameters.AddWithValue("@id_objecto", "GBICANDI");

                    command.ExecuteNonQuery(); // Executes the update command
                }
            }
        }


        private void CreateMsg(string email, int codedita)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                _logger.LogInformation("Database connection opened.");



                const string sql = @"
                INSERT INTO GBIMSG (EMAIL, CODEDITAL, MSG, TITLE, LIDA, DATAENVIO)
                OUTPUT INSERTED.EMAIL
                VALUES (@EMAIL, @CODEDITAL, @MSG, @TITLE, @LIDA, @DATAENVIO);";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@EMAIL", email);
                    command.Parameters.AddWithValue("@TITLE", "AVISO");
                    command.Parameters.AddWithValue("@MSG", "Submeta seus documentos na aba de documenttos para finalizar sua candidatura");
                    command.Parameters.AddWithValue("@LIDA", 0);
                    command.Parameters.AddWithValue("@CODEDITAL", codedita);
                    command.Parameters.AddWithValue("@DATAENVIO", DateTime.Now);
                    command.ExecuteScalar();
                    _logger.LogInformation("mensagem guardada ID");
                }
            }
        }
        private void regFiliacao(int codcandi, String nome, int codFiliacao, String filiacao, String combatente)
        {
            int combatente2= 0;
            if (combatente =="Sim")
            {
               combatente2= 1;
            }
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.LogInformation("Database connection opened.");



                    const string sql = @"
                INSERT INTO GBIFILIA (CODFILIA, FILIACAO, NOME, CODCANDI, COMBATEN)
                OUTPUT INSERTED.NOME
                VALUES (@CODFILIA, @FILIACAO, @NOME, @CODCANDI, @COMBATEN);";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@CODFILIA", codFiliacao);
                        command.Parameters.AddWithValue("@FILIACAO", filiacao);
                        command.Parameters.AddWithValue("@NOME", nome);
                        command.Parameters.AddWithValue("@CODCANDI", codcandi);
                        command.Parameters.AddWithValue("@COMBATEN", combatente2);
                        command.ExecuteScalar();
                        _logger.LogInformation("mensagem guardada ID");
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error occurred while inserting candidate data.");
                StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while inserting candidate data" + sqlEx);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting candidate data.");
                 StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred" + ex);
            }


        }

        private int[] GetInfoByLocalidade(int localid)
        {
            int[] dadosLocalidade = new int[3];

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                const string sql = @"
            SELECT CODPROVI, CODDISTR, CODZONA 
            FROM GBIPOSTO 
            WHERE CODPOSTO = @CODPOSTO";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CODPOSTO", localid);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dadosLocalidade[0] = reader.GetInt32(0); // CODPROVI
                            dadosLocalidade[1] = reader.GetInt32(1); // CODDISTR
                            dadosLocalidade[2] = reader.GetInt32(2); // CODZONA
                        }
                    }
                }
            }

            return dadosLocalidade;
        }


        private int getLastCodCandi() {
            using (var connection = new SqlConnection(_connectionString))
            {
                int lastCodCandi;
                connection.Open();
                const string sql = @"
            SELECT MAX(CODCANDI) AS MAXCOD FROM GBICANDI";

                using (var command = new SqlCommand(sql, connection))
                {
                    lastCodCandi = (int)command.ExecuteScalar();

                    return lastCodCandi;
                }
            }
        }


        [HttpPut("{codcandi}")]
        public IActionResult Put(int codcandi, [FromBody] CandidatoDTO candidato)
        {
            if (candidato == null)
            {
                _logger.LogWarning("Put request received with null candidate.");
                return BadRequest("Candidate data is null");
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.LogInformation("Database connection opened.");

                    // SQL query for updating candidate data, excluding password and email
                    const string sql = @"
                        UPDATE GBICANDI
                        SET CODPROVI = @CODPROVI,

                            TELEFONE = @TELEFONE,
                            TELEMOVE = @TELEMOVE,
                            NUMEO = @NUMEO,
                            OCUPACAO = @OCUPACAO,
                            NATURALI = @NATURALI,
                            RUA = @RUA
                        WHERE CODCANDI = @CODCANDI;
                        ";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@CODCANDI", codcandi);
                        command.Parameters.AddWithValue("@CODPROVI", candidato.codprovi);
                        command.Parameters.AddWithValue("@TELEFONE", (object)candidato.telefone ?? DBNull.Value);
                        command.Parameters.AddWithValue("@TELEMOVE", (object)candidato.telemovel ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NUMEO", (object)candidato.identificacao ?? DBNull.Value);
                        command.Parameters.AddWithValue("@OCUPACAO", (object)candidato.ocupacao ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NATURALI", (object)candidato.naturalidade ?? DBNull.Value);
                        command.Parameters.AddWithValue("@RUA", (object)candidato.rua ?? DBNull.Value);

                        // Execute the command
                        var rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            _logger.LogInformation("Candidate data updated successfully for ID {id}", codcandi);
                        }
                        else
                        {
                            _logger.LogWarning("No candidate data updated for ID {id}", codcandi);
                            return NotFound($"Candidate with ID {codcandi} not found.");
                        }
                    }
                }

                // Return a response indicating success
                var result = new
                {
                    Message = "Candidate data updated successfully",
                    Code = codcandi,
                    success = true,
                };

                return Ok(result);
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error occurred while updating candidate data.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating candidate data" + sqlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating candidate data.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred" + ex);
            }
        }

        

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromQuery] string id, IFormFile file)
        {
            // Log received parameters
            if (file != null)
            {
                Console.WriteLine($"Received ID: {id}");
                Console.WriteLine($"File: {file?.FileName}");
                Console.WriteLine($"File name: {file.FileName}");
                Console.WriteLine($"Content type: {file.ContentType}");
                Console.WriteLine($"File size: {file.Length} bytes");
            }
            else
            {
                Console.WriteLine("No file received.");
            }

            // Validate the ID parameter
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { StatusCode = 400, Message = "ID parameter is required." });
            }

            // Validate the file
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { StatusCode = 400, Message = "Ficheiro não enviado." });
            }

            try
            {
                var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", id);
                Directory.CreateDirectory(uploadsFolderPath);
                var filePath = Path.Combine(uploadsFolderPath, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Created(new Uri(filePath, UriKind.Relative), new
                {
                    StatusCode = 201,
                    Message = "Ficheiro enviado com sucesso.",
                    FilePath = filePath
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Um erro ocorreu ao enviar, contacte o IBE."
                });
            }
        }



    }
}
