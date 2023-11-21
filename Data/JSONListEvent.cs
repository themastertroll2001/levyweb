using Barberia.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Barberia.Data
{
    public class JSONListEvent
    {
        public static string GetEventListJSONString(List<Barberia.Models.Event> events)
        {
            var eventlist = new List<Event>();
            foreach (var model in events)
            {
                var myevent = new Event()
                {
                    id = model.Id,
                    start = model.Fechainicio,
                    end = model.Finfecha,
                    description = model.Descripcion,
                    title = model.Nombre
                };
                eventlist.Add(myevent);
            }
            return JsonSerializer.Serialize(eventlist);
        }
        public class Event
        {
            public int id { get; set; }
            public string title { get; set; }
            public DateTime start { get; set; }
            public DateTime end { get; set; }
            public string description { get; set; }
        }
    }
}
