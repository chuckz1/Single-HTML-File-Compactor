using System.Diagnostics;

namespace Single_HTML_File_Compactor
{
    class FileHandler
    {
        public const string newDir = "Single File";
        public const int maxStylesheets = 100;
        public const int maxScripts = 100;
        public const bool allowDebugMessages = true;

        public void main()
        {
            setupFiles();
        }

        public void setupFiles()
        {
            // To write on the console screen
            Console.Write("Enter the absolute file path: ");

            // To read the input from the user
            string filePath = Console.ReadLine();

            //trim quotes from edges of the file path
            filePath = filePath.Trim('"');

            writeLineIfDebug(allowDebugMessages, "File Path received: \"" + filePath + "\"");

            //get the file name and directory
            string fileName = Path.GetFileName(filePath);
            string workingDirectory = Path.GetDirectoryName(filePath);
            string newDirectory = workingDirectory + "\\" + newDir;
            string newFileName = newDirectory + "\\" + fileName;

            writeLineIfDebug(allowDebugMessages, "Loading File into memory");

            if (File.Exists(filePath))
            {
                //open the file
                StreamReader sr = new StreamReader(filePath);

                //go the the begining of the file
                sr.BaseStream.Seek(0, SeekOrigin.Begin);

                //get contents of the file
                string document = sr.ReadToEnd();

                //parse style sheets
                writeLineIfDebug(allowDebugMessages, "Parsing style sheets");
                document = parseStyleSheets(document, workingDirectory, allowDebugMessages);

                //parse scripts
                writeLineIfDebug(allowDebugMessages, "Parsing scripts");
                document = parseScripts(document, workingDirectory, allowDebugMessages);

                //save new file
                writeLineIfDebug(allowDebugMessages, "Creating new file in the sub folder");
                if (!File.Exists(newFileName))
                {
                    //creates directory if it doesn't exist
                    Directory.CreateDirectory(newDirectory);

                    //create file
                    StreamWriter sw = new StreamWriter(newFileName);

                    //write document to the file
                    sw.Write(document);

                    //save changes to new file
                    sw.Flush();

                    //close the new file
                    sw.Close();
                }
                else
                {
                    Console.WriteLine("New file already exists. Please delete it before running this");
                }

                //close the original file
                sr.Close();
            }
            else
            {
                Console.WriteLine("Could not find file");
            }
        }

        public string parseStyleSheets(string fileContents, string workingDirectory, bool debugMessages)
        {
            //replace style sheets
            int overflowCounter = 0;

            string searchString = "link";

            int indexTracker = fileContents.IndexOf(searchString, 0);

            while (indexTracker != -1 && overflowCounter < maxStylesheets)
            {
                //find the next <>
                int tagStart = fileContents.LastIndexOf("<", indexTracker);
                int tagEnd = fileContents.IndexOf(">", indexTracker);

                //get substring of item to delete
                string extracted = fileContents.Substring(tagStart, (tagEnd - tagStart) + 1);

                //make sure the link is a stylesheet
                if (extracted.Contains("stylesheet"))
                {
                    //find location of new content
                    int hrefIndex = extracted.IndexOf("href");

                    int stylePathStart = extracted.IndexOf("\"", hrefIndex);
                    int stylePathEnd = extracted.IndexOf("\"", stylePathStart + 1);

                    string stylePath = workingDirectory + "\\" + @extracted.Substring(stylePathStart + 1, (stylePathEnd - stylePathStart) - 1);

                    writeLineIfDebug(debugMessages, "Path of style sheet: " + stylePath);

                    //add style tag
                    string styleContent = "<style>";

                    //get content
                    if (File.Exists(stylePath))
                    {
                        //read contents of style sheet
                        StreamReader streamReader = new StreamReader(stylePath);

                        styleContent += streamReader.ReadToEnd();

                        streamReader.Close();
                    }
                    else
                    {
                        writeLineIfDebug(debugMessages, "Style sheet not found: " + stylePath);

                        styleContent += "Style sheet not found";
                    }

                    //add closing tag
                    styleContent += "</style>";


                    //remove old value
                    fileContents = fileContents.Remove(tagStart, (tagEnd - tagStart) + 1);

                    //set new value
                    fileContents = fileContents.Insert(tagStart, styleContent);

                    //find next position
                    indexTracker = fileContents.IndexOf(searchString, tagStart + (styleContent.Length));
                }
                else
                {
                    //find next position
                    indexTracker = fileContents.IndexOf(searchString, tagEnd);
                }

                overflowCounter++;
            }

            writeLineIfDebug(debugMessages, "Style Sheets replaced (max " + maxStylesheets + "): " + overflowCounter);

            //return result
            return fileContents;
        }

        public string parseScripts(string fileContents, string workingDirectory, bool debugMessages)
        {
            //replace style sheets
            int overflowCounter = 0;

            string searchString = "script";

            int indexTracker = fileContents.IndexOf(searchString, 0);

            while (indexTracker != -1 && overflowCounter < maxScripts)
            {
                //find the next <>
                int firstTagStart = fileContents.LastIndexOf("<", indexTracker);
                int firstTagEnd = fileContents.IndexOf(">", indexTracker);
                int secondTagStart = fileContents.IndexOf("<", firstTagEnd);
                int secondTagEnd = fileContents.IndexOf(">", secondTagStart);

                //get substring of item to delete
                string extracted = fileContents.Substring(firstTagStart, (firstTagEnd - firstTagStart) + 1);

                //make sure a src is provided
                if (extracted.Contains("src"))
                {
                    //find location of new content
                    int srcIndex = extracted.IndexOf("src");

                    int scriptPathStart = extracted.IndexOf("\"", srcIndex);
                    int scriptPathEnd = extracted.IndexOf("\"", scriptPathStart + 1);

                    string scriptPath = workingDirectory + "\\" + @extracted.Substring(scriptPathStart + 1, (scriptPathEnd - scriptPathStart) - 1);

                    writeLineIfDebug(debugMessages, "Path of script: " + scriptPath);


                    //add script tag
                    string scriptContent = "<script>";

                    //get content
                    if (File.Exists(scriptPath))
                    {
                        //read contents of style sheet
                        StreamReader streamReader = new StreamReader(scriptPath);

                        scriptContent += streamReader.ReadToEnd();

                        streamReader.Close();
                    }
                    else
                    {
                        writeLineIfDebug(debugMessages, "Script not found: " + scriptPath);

                        scriptContent += "Script not found";
                    }

                    //add closing tag
                    scriptContent += "</script>";


                    //remove old value
                    fileContents = fileContents.Remove(firstTagStart, (secondTagEnd - firstTagStart) + 1);

                    //set new value
                    fileContents = fileContents.Insert(firstTagStart, scriptContent);

                    //find next position
                    indexTracker = fileContents.IndexOf(searchString, firstTagStart + (scriptContent.Length));
                }
                else
                {
                    //find next position
                    indexTracker = fileContents.IndexOf(searchString, firstTagEnd);
                }

                overflowCounter++;
            }

            writeLineIfDebug(debugMessages, "Scripts replaced (max " + maxScripts + "): " + overflowCounter);

            //return result
            return fileContents;
        }

        void writeLineIfDebug(bool isDebug, string message)
        {
            if (isDebug)
            {
                Console.WriteLine(message);
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                //commands passed via command line - auto execute

                string filePath = args[0];
            }

            FileHandler handler = new FileHandler();

            handler.main();

            
            //wait so the the user can see what the program has done
            Console.WriteLine("Press enter to exit the program");
            Console.ReadLine();
        }

        //"C:\Users\charles\Desktop\container\original.html"
    }
}