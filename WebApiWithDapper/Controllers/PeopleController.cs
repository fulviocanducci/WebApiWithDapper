using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using WebApiWithDapper.Models;
using System.Linq;
namespace WebApiWithDapper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        public SqlConnection Connection { get; }
        public PeopleController(SqlConnection connection)
        {
            Connection = connection;           
        }

        // GET: api/People
        [HttpGet]
        public IEnumerable<People> Get()
        {            
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT Id, Name, PeopleId, Street, Number ");
            sql.Append("FROM People ");
            sql.Append("INNER JOIN ");
            sql.Append("Address ON People.Id = Address.PeopleId ");
            return Connection.Query<People, Address, People>(sql.ToString(), (p, a) =>
            {
                p.Address = a;
                return p;
            }, splitOn: "PeopleId");
        }

        // GET: api/People/5
        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(int id)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT Id, Name, PeopleId, Street, Number ");
            sql.Append("FROM People ");
            sql.Append("INNER JOIN ");
            sql.Append("Address ON People.Id = Address.PeopleId ");
            sql.Append("WHERE Id = @Id ");
            People data = Connection.Query<People, Address, People>(sql.ToString(), (p, a) =>
            {
                if (p != null)
                {
                    p.Address = a;
                }
                return p;
            }, splitOn: "PeopleId", param: new { Id = id })
                .FirstOrDefault(x => x.Id == id);            
            return Ok((object)data ?? new { });
        }

        // POST: api/People
        [HttpPost]
        public IActionResult Post([FromBody] People value)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO People (Name)");
            sql.Append("VALUES(@Name); SELECT SCOPE_IDENTITY()");
            value.Id = Connection.ExecuteScalar<int>(sql.ToString(), value);
            sql.Clear();
            if (value.Address != null)
            {
                sql.Append("INSERT INTO Address (PeopleId, Street, Number)");
                sql.Append("VALUES(@PeopleId, @Street, @Number)");
                value.Address.PeopleId = value.Id;
                Connection.Execute(sql.ToString(), value.Address);
            }
            return Ok(value);
        }

        // PUT: api/People/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] People value)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("UPDATE People SET Name=@Name ");
            sql.Append("WHERE Id=@Id");
            Connection.Execute(sql.ToString(), new { value.Id, value.Name });
            sql.Clear();
            if (value.Address != null)
            {
                sql.Append("UPDATE Address SET Street=@Street, Number=@Number ");
                sql.Append("WHERE PeopleId=@PeopleId");
                value.Address.PeopleId = value.Id;
                Connection.Execute(sql.ToString(), value.Address);
            }
            return Ok(value);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("DELETE FROM Address WHERE PeopleId=@Id;");
            sql.Append("DELETE FROM People WHERE Id=@Id");
            int rowsAffected = Connection.Execute(sql.ToString(), new { Id = id });
            return Ok(new { affected = rowsAffected, status = rowsAffected > 0 });
        }


        [HttpGet("phones")]
        public IEnumerable<People> GetPhones()
        {
            //StringBuilder sql = new StringBuilder();
            //sql.Append("SELECT People.Id, People.Name, Phone.Id, Phone.PeopleId, Phone.Ddd, Phone.Number ");
            //sql.Append("FROM People ");
            //sql.Append("INNER JOIN ");
            //sql.Append("Phone ON People.Id = Phone.PeopleId ");
            //List<People> datas = new List<People>();
            //Connection.Query<People, Phone, People>(sql.ToString(), (p, ph) =>
            //{
            //    if (p.Phones == null) p.Phones = new List<Phone>();
            //    if (p?.Id == ph?.PeopleId) p.Phones.Add(ph);
            //    var r = datas.FirstOrDefault(x => x.Id == p.Id);
            //    if (r != null)
            //    {
            //        r.Phones.Add(ph);
            //    } else
            //    {
            //        datas.Add(p);
            //    }
            //    return p;
            //}, splitOn: "Id, Id");
            //return datas;


            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT * FROM People;");
            sql.Append("SELECT * FROM Phone AS ph WHERE ");
            sql.Append("EXISTS(SELECT * FROM People AS p ");
            sql.Append("WHERE p.Id = ph.PeopleId);");
            var reader = Connection.QueryMultiple(sql.ToString());
            List<People> peoples = reader.Read<People>().ToList();
            List<Phone> phones = reader.Read<Phone>().ToList();
            if (phones != null && phones.Count > 0)
            {
                peoples.ForEach(c =>
                {
                    c.Phones = phones
                        .Where(p => p.PeopleId == c.Id)
                        .ToList();
                });
            }
            return peoples;
        }
    }
}
