using System.Diagnostics;

if (args.Length < 1)
{
    //Console.WriteLine("Usage: FStarBisect %filename.fst%");
    //Console.WriteLine("The first error line will be sent to STDOUT.");
    //return;
    args = new string[] { "Sandbox.fst" };
}
List<int> validSearchIndices = new List<int> { 0 };
string[] fileLines;
try
{
    fileLines = File.ReadAllLines(args[0]);
    if (fileLines.Length < 1)
    {
        Console.WriteLine("Empty Input File...");
        return;
    }
}
catch (Exception ex)
{
    Console.WriteLine("Error reading file!");
    Console.WriteLine(ex.ToString());
    return;
}

string[] validStarts = { "assume ", "val ", "let ", "private ", "unfold ", "irreducible ", "#push-options ", "#pop-options ", "type ", "class ", "open ", "module ", "[@" };

for(int i = 0; i< fileLines.Length; i++)
{
    if (validStarts.Any(fileLines[i].StartsWith) && i > 0) validSearchIndices.Add(i);
}
validSearchIndices.Add(fileLines.Length);

string Unquote(string what) => (what.StartsWith("\"") && what.EndsWith("\"") ? what.Substring(1, what.Length - 2) : what);
string temp = Unquote(args[0]);
string oldModuleName = new FileInfo(args[0]).Name;
void SetModuleName(string oldName, string newName, int extensionLength)
{
    var index = Array.FindIndex(fileLines, line => line.Contains("module ") && !line.Contains("="));
    if (index > -1) fileLines[index] = "module " + newName.Substring(0, newName.Length - extensionLength);
    oldModuleName = newName;
}
do
{
    var fileInfo = new FileInfo(temp);
    if (fileInfo.Directory == null)
    {
        Console.WriteLine("Wrong directory!");
        return;
    }
    
    string newName = fileInfo.Name;
    if (fileInfo.Name.EndsWith(".fst"))
    {
        newName = fileInfo.Name.Substring(0, fileInfo.Name.Length - 4) + ".Bisect.fst";
        SetModuleName(oldModuleName, newName, 4);
    }
    else if (fileInfo.Name.EndsWith(".fsti"))
    {
        newName = fileInfo.Name.Substring(0, fileInfo.Name.Length - 5) + ".Bisect.fsti";
        SetModuleName(oldModuleName, newName, 5);
    }
    else
    {
        Console.WriteLine("Bad input file extension");
        return;
    }
    temp = Path.Combine(fileInfo.Directory.FullName, newName);

} while (File.Exists(temp));

string quotedTemp = !temp.StartsWith("\"") ? $"\"{temp}\"" : temp;
bool lastCheck = false;
bool IsValid(int searchLineIndex)
{
    if (searchLineIndex >= validSearchIndices.Count) searchLineIndex = validSearchIndices.Count - 1;
    File.WriteAllLines(temp, fileLines.Take(validSearchIndices[searchLineIndex]));
    // Console.WriteLine("Last Line: ");
    // Console.WriteLine(fileLines[validSearchIndices[searchLineIndex] - 1]);

    var ps = new ProcessStartInfo("FStar", $"--admit_smt_queries true {quotedTemp}")
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    var process = Process.Start(ps); 
    process.WaitForExit();
    lastCheck = process.ExitCode == 0;
    return (process.ExitCode == 0); 
}

int GetErrorLine(int index, int indexDelta)
{
    if (validSearchIndices.Count <= index)
        Console.WriteLine("Trying the whole file");
    else
        Console.WriteLine("Trying at line " + validSearchIndices[index]);
    if (IsValid(index))
    {
        if (index >= validSearchIndices.Count)
        {
            return validSearchIndices.Count;
        }
        if (indexDelta == 0) return index;
        return GetErrorLine(index + indexDelta, indexDelta * 2 / 3);
    }
    else
    {
        if (indexDelta == 0) return index;
        return GetErrorLine(index - indexDelta, indexDelta * 2 / 3);
    }
}

var errorLine = GetErrorLine(validSearchIndices.Count, validSearchIndices.Count / 2);
if (errorLine >= validSearchIndices.Count) Console.WriteLine("No errors found.");
else
{
    if (!lastCheck)
        errorLine--;
    if (errorLine < 0) errorLine = 0;
    Console.WriteLine("Error Line: {0}", validSearchIndices[errorLine]);
}
File.Delete(temp);