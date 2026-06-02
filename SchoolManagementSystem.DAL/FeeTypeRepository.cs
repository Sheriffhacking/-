using Microsoft.Data.SqlClient;
using SchoolManagementSystem.Models;
using System.Collections.Generic;

namespace SchoolManagementSystem.DAL
{
    public class FeeTypeRepository
    {
        private readonly DatabaseConnection db =
            new DatabaseConnection();

        public List<FeeType> GetAll()
        {
            List<FeeType> list = new();

            using var conn = db.GetConnection();

            conn.Open();

            string query =
                "SELECT * FROM FeeTypes";

            using SqlCommand cmd =
                new SqlCommand(query, conn);

            using SqlDataReader reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new FeeType
                {
                    FeeTypeId =
                        (int)reader["FeeTypeId"],

                    FeeName =
                        reader["FeeName"].ToString()
                });
            }

            return list;
        }
    }
}