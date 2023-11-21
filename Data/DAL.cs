using Barberia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Barberia.Data
{
    public interface IDAL
    {
        List<Event> GetEvents();
   
        Event? GetEvent(int id);
        void CreateEvent(Event myEvent);  // Modificado para aceptar Event
        void UpdateEvent(Event myEvent);  // Modificado para aceptar Event
        void DeleteEvent(int id);
       
    }

    public class DAL : IDAL
    {
        private readonly BdBarberiaContext _db;

        public DAL(BdBarberiaContext db)
        {
            _db = db;
        }

        public List<Event> GetEvents()
        {
            return _db.Events.ToList();
        }

      

        public Event? GetEvent(int id)
        {
            return _db.Events.FirstOrDefault(x => x.Id == id);
        }

        public void CreateEvent(Event myEvent)
        {
            try
            {
                _db.Events.Add(myEvent);
                _db.SaveChanges();
            }
            catch (DbUpdateException ex) // Esto captura las excepciones específicas de EF al actualizar la DB.
            {
                // Aquí puedes registrar o mostrar el mensaje de la excepción y su InnerException.
                var errorMessage = ex.Message;
                var innerMessage = ex.InnerException?.Message;

                // Por ahora, simplemente lanzaremos la excepción con ambos mensajes para ver qué sucede.
                throw new Exception($"Error: {errorMessage}. Inner Exception: {innerMessage}");
            }
        }


        public void UpdateEvent(Event myEvent)  // Modificado para aceptar Event
        {
            _db.Entry(myEvent).State = EntityState.Modified;
            _db.SaveChanges();
        }

        public void DeleteEvent(int id)
        {
            var myEvent = _db.Events.Find(id);
            if (myEvent != null)
            {
                _db.Events.Remove(myEvent);
                _db.SaveChanges();
            }
        }
       
    }
}