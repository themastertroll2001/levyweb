using Barberia.Models;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

namespace Barberia.Data
{
    public class DbContext
    {
        public DbContext(string valor) => Valor = valor;
        public string Valor { get;}
       


    }
}
