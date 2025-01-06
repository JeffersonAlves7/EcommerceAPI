using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ItemsController : ControllerBase
	{
		private readonly IConfiguration _configuration;

		public ItemsController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		// GET: api/<ItemsController>
		[HttpGet]
		public IActionResult Get()
		{
			using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DbCon").ToString());
			SqlDataAdapter data = new SqlDataAdapter("SELECT * FROM dbo.Product", con);

			DataTable dt = new DataTable();
			data.Fill(dt);

			List<Product> products = new List<Product>();

			foreach (DataRow dr in dt.Rows)
			{
				products.Add(new Product
				{
					Id = Convert.ToInt32(dr["Id"]),
					Name = Convert.ToString(dr["Name"]),
					Description = Convert.ToString(dr["Description"])
				});
			}

			return Ok(products); // Retorna como JSON
		}

		// GET api/<ItemsController>/5
		[HttpGet("{id}")]
		public IActionResult Get(int id)
		{
			using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DbCon").ToString());
			SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.Product WHERE Id = @Id", con);
			cmd.Parameters.AddWithValue("@Id", id);

			DataTable dt = new DataTable();
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			da.Fill(dt);

			if (dt.Rows.Count == 0)
				return NotFound("Product not found");

			DataRow dr = dt.Rows[0];
			Product product = new Product
			{
				Id = Convert.ToInt32(dr["Id"]),
				Name = Convert.ToString(dr["Name"]),
				Description = Convert.ToString(dr["Description"])
			};

			return Ok(product);
		}

		// POST api/<ItemsController>
		[HttpPost]
		public IActionResult Post([FromBody] Product product)
		{
			using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DbCon").ToString());
			con.Open();

			// Usando apenas Name e Description, pois o Id será gerado automaticamente pelo banco
			SqlCommand cmd = new SqlCommand(
				"INSERT INTO dbo.Product (Name, Description) VALUES (@Name, @Description); SELECT SCOPE_IDENTITY();",
				con
			);

			cmd.Parameters.AddWithValue("@Name", product.Name);
			cmd.Parameters.AddWithValue("@Description", product.Description);

			// Obtém o Id gerado pelo banco
			int newId = Convert.ToInt32(cmd.ExecuteScalar());

			// Atualiza o produto com o novo Id
			product.Id = newId;

			// Retorna o produto completo
			return Ok(product);
		}


		// PUT api/<ItemsController>/5
		[HttpPut("{id}")]
		public IActionResult Put(int id, [FromBody] Product product)
		{
			using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DbCon").ToString());
			con.Open();

			SqlCommand cmd = new SqlCommand("UPDATE dbo.Product SET Name = @Name, Description = @Description WHERE Id = @Id", con);
			cmd.Parameters.AddWithValue("@Id", id);
			cmd.Parameters.AddWithValue("@Name", product.Name);
			cmd.Parameters.AddWithValue("@Description", product.Description);

			int rowsAffected = cmd.ExecuteNonQuery();
			if (rowsAffected == 0)
				return NotFound("Product not found");

			return NoContent();
		}

		// DELETE api/<ItemsController>/5
		[HttpDelete("{id}")]
		public IActionResult Delete(int id)
		{
			using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DbCon").ToString());
			con.Open();

			SqlCommand cmd = new SqlCommand("DELETE FROM dbo.Product WHERE Id = @Id", con);
			cmd.Parameters.AddWithValue("@Id", id);

			int rowsAffected = cmd.ExecuteNonQuery();
			if (rowsAffected == 0)
				return NotFound("Product not found");

			return NoContent();
		}
	}
}
