using Nancy.Json;
using StartshipsStops.model;
using StartshipsStops.web.api;
using System;
using System.Collections.Generic;

namespace StartshipsStops
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello! this software calculate  the amount of stops required to make the distance between the planets for each spaceship listed");
            Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");

            try
            {
                //new instance of the connector to REST API
                ApiClient api = new ApiClient("http://swapi.dev/api");

                List<Spaceship> spaceships = new List<Spaceship>();


                int distance = 0;

                //Set and validate de distance, if the entry is invalid it ask to retry.
                Console.WriteLine("Enter with the distance beteween the planets in mega lights MGLT... Only integer and positive numbers:");
            setDistance: while (!int.TryParse(Console.ReadLine(), out distance))
                {
                    Console.WriteLine("ATENTION! Enter with a valid integer and positive number:");
                }
                if (distance < 1)
                {
                    Console.WriteLine("ATENTION! Enter with a valid integer and positive number:");
                    goto setDistance;
                }

                Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("Searching Spaceships...");
                JavaScriptSerializer jSerializer = new JavaScriptSerializer();

                //Consume API and deserialize the json to a object until there is no next page.
                spaceships.Add(
                       (Spaceship)jSerializer.Deserialize<Spaceship>(api.GetApiResponse("starships/")));

                while (!string.IsNullOrWhiteSpace(spaceships[spaceships.Count - 1].next))
                {
                    string[] nextMethod = spaceships[spaceships.Count - 1].next.Split('/');
                    spaceships.Add(
                      (Spaceship)jSerializer.Deserialize<Spaceship>(api.GetApiResponse("starships/" + nextMethod[nextMethod.Length - 1])));
                }
                Console.WriteLine(spaceships[0].count + " spaceships found.");
                Console.WriteLine("Calculating...");

                //Calculating the amount of stops
                List<Stops> stops = calculateStops(distance, spaceships);
                Console.WriteLine("Stops calculated Successfuly");
                Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");

                //sort the final array by name or amount of stops
                Console.WriteLine("Type de Order By code:\n1 - Name\n2 - Amount of stops");
                string orderOption = Console.ReadLine();
                while (orderOption != "1" && orderOption != "2")
                {
                    Console.WriteLine("Type de valid Order By code:\n1 - Name\n2 - Amount of stops");
                    orderOption = Console.ReadLine();
                }
                if (orderOption == "1")
                    stops.Sort((x, y) => x.SpaceshipName.CompareTo(y.SpaceshipName));
                else
                {

                    stops.Sort((x, y) => y.AmountStops.CompareTo(x.AmountStops));
                }


                //Printing the final result
                Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("Spaceship - Amount of stops\n");
                foreach (var stop in stops)
                {
                    //if the amount is <0 it will show unknown, because it was impossible to be calculated.
                    string quantity = "unknown";
                    if (stop.AmountStops > 0)
                    {
                        quantity = stop.AmountStops.ToString();
                    }
                    Console.WriteLine(string.Format("{0} - {1}", stop.SpaceshipName, quantity));
                }
                Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("END!");

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }
        }

 
        /// <summary>
        /// This method calculates the amount of stops for each spaceship based on the distance beteween two planets  
        /// </summary>
        /// <param name="distance">Distance in MGTL</param>
        /// <param name="spaceships">Spaceships from the API</param>
        /// <returns></returns>
        static List<Stops> calculateStops(int distance, List<Spaceship> spaceships)
        {
            List<Stops> ret = new List<Stops>();

            try
            {
                
                foreach (var spaceshipItem in spaceships)
                {
                    foreach (var spaceship in spaceshipItem.results)
                    {
                        int stops = -1;
                        //validating if the consumables and mglt are known, if not the amount of stops is set -1
                        if (spaceship.consumables != "unknown"
                            && !string.IsNullOrWhiteSpace(spaceship.consumables)
                            && spaceship.MGLT != "unknown"
                            && !string.IsNullOrWhiteSpace(spaceship.MGLT)
                            )
                        {
                            //converting the consumables period to days.
                            string[] consumables = spaceship.consumables.Trim().Split(' ');
                            int consumableDays = CalculateConsumablesDays(int.Parse(consumables[0]), consumables[1]);
                           
                            //calculating the amount of stops
                            //MGLT is the distance per hour, so 24 times because a day have 24 hours
                            stops = distance / (consumableDays * ((int.Parse(spaceship.MGLT) * 24)));

                        }
                        ret.Add(new Stops { SpaceshipName = spaceship.name, AmountStops = stops });
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return ret;

        }

        /// <summary>
        /// This method converts weeks, months and years to days.
        /// </summary>
        /// <param name="number">Unit in the original type</param>
        /// <param name="type">types: "day", "days", "week", "weeks", "month", "months", "year" and "years"</param>
        /// <returns>return the number of days</returns>
        static int CalculateConsumablesDays(int number, string type)
        {
            type = type.ToLower();
            int days = 0;
            if (type.Contains("day"))
            {
                days = number;
            }
            else if (type.Contains("year"))
            {
                days = number * 365;
            }
            else if (type.Contains("month"))
            {
                days = number * 30;
            }
            else if (type.Contains("week"))
            {
                days = number * 7;
            }
            else
            {
                throw new Exception("Erro converting consumables, type is unknown");
            }

            return days;
        }
    }
}
