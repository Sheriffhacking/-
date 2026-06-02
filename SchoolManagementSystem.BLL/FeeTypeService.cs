using SchoolManagementSystem.DAL;
using SchoolManagementSystem.Models;
using System.Collections.Generic;

namespace SchoolManagementSystem.BLL
{
    public class FeeTypeService
    {
        private readonly FeeTypeRepository repo =
            new FeeTypeRepository();

        public List<FeeType> GetAll()
        {
            return repo.GetAll();
        }
    }
}