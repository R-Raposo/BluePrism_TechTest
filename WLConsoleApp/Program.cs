using System;
using System.IO;
using System.Threading.Tasks;
using RicardoRaposo.DotNetWordLadderSolver;

namespace WordLadderApp
{
    class Program
    {
        static void Main(string[] args)
        {
            BeginNewGame();
        }

        private static void BeginNewGame()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            BeginGameIntro();
            //Console.ForegroundColor = ConsoleColor.White;
            ReadGameParameters(out string firstWord, out string lastWord, out string[] wordDictionary, out string resultFilePath);

            // we can use the WorldLadderSolver class as static or as an instance
            // when used as an instance, we have different methods available, to use different algorithms to solve the word ladder
            WordLadderSolutionBuilder wordLadderSolver = new WordLadderSolutionBuilder(firstWord, lastWord, wordDictionary);
            wordLadderSolver.GetSolution();

            string solution = WordLadderSolutionBuilder.GetSolution(firstWord, lastWord, wordDictionary);

            string alternativePath = WriteResultToFile(resultFilePath, Path.GetFileName(resultFilePath), solution);

            Console.Write("The solution is this: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(solution);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Game over!");

            if(string.IsNullOrWhiteSpace(alternativePath) == false)
            {
                // something went wrong writing the file to the path the user has inputted.
                // the result file was created in an alternative path, so we should probably warn the user the file is somewhere else
                Console.Write("Unable to write result file to the path provided, the file was alternatively created in the following path: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(alternativePath, Console.ForegroundColor);
            }

            Console.ReadLine();
        }

        private static void BeginGameIntro()
        {
            Console.WriteLine("Welcome to the Word Ladder Game");
        }

        private static void ReadGameParameters(out string firstWord, out string lastWord, out string[] wordDictionary, out string resultFilePath)
        {
            RequestWordDictionary("Please write the word dictionary file path: ", out wordDictionary);

            RequestWordParameter("Please write the first word of the ladder: ", wordDictionary, out firstWord);

            RequestWordParameter("Please write the last word of the ladder: ", wordDictionary, out lastWord);

            RequestResultDirectory("Please write path where you would like the file containing the result to be created: ", out resultFilePath);
        }

        private static void RequestResultDirectory(string inputRequest, out string resultFilePath)
        {
            Console.WriteLine(inputRequest);
            ReadParameter(out resultFilePath);
        }

        private static void RequestWordDictionary(string inputRequest, out string[] wordDictionary)
        {
            bool fileValuesValid = false;
            do
            {
                ReadWordDictionaryFilePath(inputRequest, out string filePath);

                ReadWordDictionaryFile(filePath, out wordDictionary);

                if (wordDictionary.Length <= 1)
                {
                    Console.WriteLine("The file does not have enough word. Please choose another file.");
                }
                else
                {
                    fileValuesValid = true;
                }

            } while (fileValuesValid == false);
        }

        private static void RequestWordParameter(string inputRequest, string[] wordDictionary, out string wordInput)
        {
            bool wordValid = false;
            do
            {
                Console.WriteLine(inputRequest);
                ReadParameter(out wordInput);

                wordInput = wordInput.ToLower();

                string validationResult = ValidateGameWord(wordInput, wordDictionary);

                if (string.IsNullOrWhiteSpace(validationResult) == false)
                {
                    // word is not valid, show error and ask for another
                    Console.WriteLine(validationResult);
                }
                else
                {
                    // word is valid, break the loop
                    wordValid = true;
                }
            }
            while (wordValid == false);
        }

        /// <summary>
        /// Handles the user input for the file path argument, and loads the file
        /// </summary>
        /// <param name="inputRequest"></param>
        /// <param name="wordDictionary"></param>
        private static void ReadWordDictionaryFilePath(string inputRequest, out string wordDictionaryFilePath)
        {
            bool fileValid = false;
            do
            {
                Console.WriteLine(inputRequest);
                ReadParameter(out wordDictionaryFilePath);

                string validationResult = ValidateFilePath(wordDictionaryFilePath);

                if (string.IsNullOrWhiteSpace(validationResult) == false)
                {
                    // file path is not valid, show error and ask for another
                    Console.WriteLine(validationResult);
                }
                else
                {
                    fileValid = true;
                }
            }
            while (fileValid == false);
        }

        private static void ReadWordDictionaryFile(string filePath, out string[] wordDictionary)
        {
            wordDictionary = LoadFileAsync(filePath).Result;
        }

        private static void ReadParameter(out string parameter)
        {
            ConsoleColor uiColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.White;
            parameter = Console.ReadLine();
            Console.ForegroundColor = uiColor;
            return;
        }

        private static string ValidateGameWord(string word, string[] wordDictionary)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return "Word is not filled"; //ERROR
            }

            if (word.Length != 4)
            {
                return "Word must be 4 characters long"; //ERROR
            }

            if (Array.IndexOf(wordDictionary, word) == -1)
            {
                return string.Format("{0} is not present on the dictionary", word);
            }

            return ""; //SUCCESS
        }

        private static string ValidateFilePath(string filePath)
        {
            if(string.IsNullOrWhiteSpace(filePath))
            {
                return "File path is not filled"; //ERROR
            }

            if(File.Exists(filePath) == false)
            {
                return "File not found"; //ERROR
            }

            return ""; //SUCCESS
        }

        private static async Task<string[]> LoadFileAsync(string filePath)
        {
            try
            {
                var fileText = await File.ReadAllLinesAsync(filePath);

                if (fileText.Length == 0)
                {
                    //File empty?
                    return new string[0];
                }
                else
                {
                    return fileText;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Writes file to designated path, returns a string with the alternative path where the file was created if anything goes wrong
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        private static string WriteResultToFile(string filePath, string fileName, string fileContent)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "result.txt";
            }

            if (string.IsNullOrWhiteSpace(Path.GetExtension(fileName)))
            {
                fileName += ".txt";
            }

            try
            {
                Directory.CreateDirectory(Path.Combine(filePath));
                File.AppendAllText(Path.Combine(filePath, fileName), fileContent);
                return string.Empty;
            }
            catch (Exception)
            {
                // Something went wrong trying to write the file in the path
                // Write the file somewhere else

                string alternativeFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "WordLadderResult");

                Directory.CreateDirectory(alternativeFilePath);
                File.AppendAllText(Path.Combine(alternativeFilePath, fileName), fileContent);
                return alternativeFilePath;
            }
        }
    }
}
