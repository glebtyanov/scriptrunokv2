using System.IO;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;

namespace ScriptrunokV2
{
    public class Coords
    {
        public int[] Lot1Nakleika { get; set; } = [1495, 583];
        public int[] Kupit { get; set; } = [2155, 587];
        public int[] Galochka { get; set; } = [989, 459];
        public int[] Podtverdit { get; set; } = [1198, 747];
        public int[] Nazad { get; set; } = [302, 935];
        public int[] Ok { get; set; } = [1228, 739739];
        public int VisotaSlota { get; set; } = 144;
        public int SlotsColvo { get; set; } = 7;
    }

    public class Colors
    {
        public List<int> Fon { get; set; } = [3, 10800000];

        public int[] Galochka { get; set; } = [2000000, 7688999];

        public int[] Ok { get; set; } = [8000000, 11000000];

        public int[] Nazad { get; set; } = [4429848, 7229842];
    }

    public class Sleeps
    {
        public int PosleNazad { get; set; } = 300;
        public int PoslePodtverdit { get; set; } = 300;
        public int PosleGalochki { get; set; } = 300;
        public int ChastotaZaprosov { get;  set; } = 6000;
        public int PosleKupit { get; set;  } = 300;
    }

    public class Config
    {
        public Config()
        {
            string[] fileNames =
            [
                "./Coordinati.json",
                "./Cveta.json",
                "./Zaderzhki.json"
            ];


            for (var i = 0; i < fileNames.Length; i++)
            {
                var filePath = fileNames[i];
                if (File.Exists(filePath))
                {
                    try
                    {
                        var jsonContent = File.ReadAllText(filePath);

                        switch (i)
                        {
                            case 0:
                            {
                                var coords = JsonConvert.DeserializeObject<Coords>(jsonContent);
                                if (coords is null)
                                {
                                    Coords = new Coords();
                                    throw new JsonException();
                                }

                                Coords = coords;
                                break;
                            }

                            case 1:
                            {
                                var colors = JsonConvert.DeserializeObject<Colors>(jsonContent);
                                
                                if (colors is null)
                                {
                                    Colors = new Colors();
                                    throw new JsonException();
                                }
                                
                                Colors = colors;
                                break;
                            }

                            case 2:
                            {
                                var sleeps = JsonConvert.DeserializeObject<Sleeps>(jsonContent);
                                
                                if (sleeps is null)
                                {
                                    Sleeps = new Sleeps();
                                    throw new JsonException();
                                }
                                
                                Sleeps = sleeps;
                                break;}
                        }
                    }
                    catch 
                    {
                        Console.WriteLine(
                            "Произошла ошибка при чтении файла конфигурации. Взяты значения по умолчанию");
                    }
                }
                else
                {
                    Console.WriteLine("Файл конфигурации не найден.");
                    Colors = new();
                    Coords = new();
                    Sleeps = new();
                }
            }
        }

        public Coords Coords { get; set;  }
        public Colors Colors { get; set;  }
        public Sleeps Sleeps { get; set;  }
    }
}