//Eksempel på funktionel kodning hvor der kun bliver brugt et model lag
namespace Plukliste;

class PluklisteProgram
{
    private static ConsoleColor standardColor = Console.ForegroundColor;
    private static char readKey = ' ';
    private static List<string>? files = new List<string>();
    private static int fileIndex = -1;
    private static Pluklist? plukliste;
    private static string currentFileType = "";

    static void Main()
    {
        //Arrange
        Directory.CreateDirectory("import");
        if (!Directory.Exists("export"))
        {
            Console.WriteLine("Directory \"export\" not found");
            Console.ReadLine();
            return;
        }

        files = Directory.EnumerateFiles("export").ToList();

        //ACT
        while (readKey != 'Q')
        {
            if (files.Count == 0)
            {
                Console.WriteLine("No files found.");

            }
            else
            {
                if (fileIndex == -1)
                {
                    fileIndex = 0;
                }

                Console.WriteLine($"Plukliste {fileIndex + 1} af {files.Count}");
                Console.WriteLine($"\nfile: {files[fileIndex]}");

                //read file
                FileStream file = File.OpenRead(files[fileIndex]);
                System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(Pluklist));

                plukliste = (Pluklist?)xmlSerializer.Deserialize(file);

                //print plukliste
                if (plukliste != null && plukliste.Lines != null)
                { 
                    Console.WriteLine("\n{0, -13}{1}", "Name:", plukliste.Name);
                    Console.WriteLine("{0, -13}{1}", "Forsendelse:", plukliste.Forsendelse);

                    Console.WriteLine("\n{0,-7}{1,-9}{2,-20}{3}", "Antal", "Type", "Produktnr.", "Navn");
                    foreach (Item item in plukliste.Lines)
                    {
                        if (item.Type == ItemType.Print)
                        {
                            currentFileType = item.ProductID;
                        }
                        Console.WriteLine("{0,-7}{1,-9}{2,-20}{3}", item.Amount, item.Type, item.ProductID, item.Title);
                    }
                }
                file.Close();
            }

            //Print options
            PrintOptions();

            // ReadKey
            readKey = Console.ReadKey().KeyChar;
            readKey = Char.ToUpper(readKey);
            Console.Clear();

            // Actions
            Actions();
            Console.ForegroundColor = standardColor; //reset color

        }
    }
    static void MakeFirstLetterInStringColored(string text, ConsoleColor letterColor = ConsoleColor.Green)
    {
        string firstLetter = text.Substring(0, 1);
        string rest = text.Remove(0, 1);

        Console.ForegroundColor = letterColor;
        Console.Write(firstLetter);

        Console.ForegroundColor = standardColor;
        Console.WriteLine(rest);
    }
    static void PrintOptions()
    {
        Console.WriteLine("\n\nOptions:");
        MakeFirstLetterInStringColored("Quit");

        if (fileIndex >= 0)
        {
            MakeFirstLetterInStringColored("Afslut plukseddel");
        }
        if (fileIndex > 0)
        {
            MakeFirstLetterInStringColored("Forrige plukseddel");
        }
        if (fileIndex < files.Count - 1)
        {
            MakeFirstLetterInStringColored("Næste plukseddel");
        }

        MakeFirstLetterInStringColored("Genindlæs pluksedler");
        MakeFirstLetterInStringColored("Print plukseddel");
    }
    static void Actions()
    {
        switch (readKey)
        {
            case 'G':
                files = Directory.EnumerateFiles("export").ToList();
                fileIndex = -1;

                Console.ForegroundColor = ConsoleColor.Red; //status in red
                Console.WriteLine("Pluklister genindlæst");
                break;
            case 'F':
                if (fileIndex > 0) fileIndex--;
                break;
            case 'N':
                if (fileIndex < files.Count - 1) fileIndex++;
                break;
            case 'A':
                //Move files to import directory
                string fileWithoutPath = files[fileIndex].Split("/")[1];

                File.Move(files[fileIndex], string.Format(@"import//{0}", fileWithoutPath));

                Console.WriteLine($"Plukseddel {files[fileIndex]} afsluttet.");

                files.Remove(files[fileIndex]);
                if (fileIndex == files.Count) fileIndex--;
                break;
            case 'P':
                string currentFile = files[fileIndex].Split("/")[1];
                currentFile = currentFile.Replace(".XML", "");
                string htmlPath = $"print/{currentFile}.html";

                string template = $"templates/{currentFileType}.html";
                string templateData = File.ReadAllText(template);

                templateData = templateData.Replace("[Name]", plukliste.Name);

                string linesSetup = "<div>";
                linesSetup += "<div>Antal</div>";
                linesSetup += "<div>Type</div>";
                linesSetup += "<div>Produktnr.</div>";
                linesSetup += "<div>Navn</div>";

                foreach (Item item in plukliste.Lines)
                {
                    templateData = templateData.Replace("[Adresse]", plukliste.Adresse);

                    if (item.Type != ItemType.Print)
                    {
                        linesSetup += $"<div>{item.Amount.ToString()}</div>";
                        linesSetup += $"<div>{item.Type.ToString()}</div>";
                        linesSetup += $"<div>{item.ProductID}</div>";
                        linesSetup += $"<div>{item.Title}</div>";
                    }
                }

                linesSetup += "</div>";

                templateData = templateData.Replace("[Plukliste]", linesSetup);

                File.WriteAllText(htmlPath, templateData);

                break;
        }
    }
}
