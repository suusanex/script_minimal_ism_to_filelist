#r "nuget: System.Xml.Linq"
#r "nuget: CsvHelper, 27.2.1" // 古い実装を使用しているが実害は無いため、バージョンを固定して運用する

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CsvHelper;
using System.Globalization;

// if (Args.Length < 2)
// {
//     Console.WriteLine("Usage: dotnet-script main.csx <inputFilePath> <outputFilePath>");
//     return;
// }

// string inputFilePath = Args[0];
// string outputFilePath = Args[1];

string inputFilePath = @"D:\WorkingCopy\Passage\passage\InstallShield\ism\Flex Work Place Passage CIFS x64\Flex Work Place Passage CIFS x64.ism";
string outputFilePath = @"D:\OneDriveB\サイエンスパーク株式会社\SPC-FWP - Passage - Passage\Docs\解析\filelist.csv";


try
{
    var document = XDocument.Load(inputFilePath);

    // Parse environment variables
    var envVariables = document.Root
        .Elements("table")
        .FirstOrDefault(t => t.Attribute("name")?.Value == "ISPathVariable")?
        .Elements("row")
        .ToDictionary(
            row => row.Elements("td").ElementAt(0).Value,
            row =>
            {
                var value = row.Elements("td").ElementAt(1).Value;
                return string.IsNullOrEmpty(value) || int.TryParse(value, out _) ? $"<{row.Elements("td").ElementAt(0).Value}>" : value;
            });

    // Log envVariables after initial creation
    Console.WriteLine("Initial envVariables:");
    foreach (var kvp in envVariables)
    {
        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
    }

    // Resolve environment variables recursively
    bool hasUnresolvedVariables;
    do
    {
        hasUnresolvedVariables = false;
        for (int i = 0; i < envVariables.Count; i++)
        {
            var pair = envVariables.ElementAt(i);
            // Check if the value contains any unresolved variables
            if (!pair.Value.Contains('<') || !pair.Value.Contains('>'))
            {
                continue;
            }

            //値の中に、自分の要素以外の環境変数キーが含まれている場合のみ続行。ただし、その環境変数キーが変換不可の状態（このスクリプト独自の仕様で、値が<キー>の形式）である場合はスキップ
            var matchPair = envVariables.FirstOrDefault(x => pair.Value.Contains($"<{x.Key}>"));
            if (matchPair.Equals(default(KeyValuePair<string, string>)) || 
                matchPair.Key == pair.Key ||
                matchPair.Value == $"<{matchPair.Key}>")
            {
                continue;
            }

            var prevValue = string.Copy(pair.Value);
            envVariables[pair.Key] = pair.Value.Replace($"<{matchPair.Key}>", matchPair.Value);
            Console.WriteLine($"Resolving {pair.Key}: {prevValue} -> {envVariables[pair.Key]}");
            hasUnresolvedVariables = true;
        }
    } while (hasUnresolvedVariables);

    // Log envVariables after resolution
    Console.WriteLine("Resolved envVariables:");
    foreach (var kvp in envVariables)
    {
        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
    }

    // Parse file list
    var files = document.Root
        .Elements("table")
        .FirstOrDefault(t => t.Attribute("name")?.Value == "File")?
        .Elements("row")
        .Select(row =>
        {
            var fileName = row.Elements("td").ElementAt(2).Value.Split('|').Last();
            var relativePath = row.Elements("td").ElementAt(8).Value;

            // Resolve environment variables in the relative path
            foreach (var envVar in envVariables)
            {
                relativePath = relativePath.Replace($"<{envVar.Key}>", envVar.Value);
            }

            return new { FileName = fileName, RelativePath = relativePath };
        });

    // Write to CSV
    using (var writer = new StreamWriter(outputFilePath))
    // Update CsvWriter instantiation to use CsvWriterOptions
    using (var csv = new CsvWriter(writer, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)))
    {
        csv.WriteRecords(files);
    }

    Console.WriteLine("File list successfully written to " + outputFilePath);
}
catch (Exception ex)
{
    Console.WriteLine("Error: " + ex.ToString());
}