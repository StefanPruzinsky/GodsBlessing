using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace GodsBlessing
{
    public enum ConfigurationFile
    {
        ApplicationConfiguration,
        DatabaseConfiguration,
        RoutingTableConfiguration
    }

    class FileSystemHelper
    {
        public StringHelper StringHelper { get; private set; }

        public FileSystemHelper()
        {
            StringHelper = new StringHelper();
        }

        public void CreateFile(string typeOfFile, string nameOfFile, string content)
        {
            string correctTypeName = "";

            if (typeOfFile != "dbtemplate")
                correctTypeName = StringHelper.FirstLetterToUpper(typeOfFile);
            else
                correctTypeName = "DBTemplate";

            if (Array.IndexOf((new string[] { "Layout", "View", "Component" }), typeOfFile) != -1)
                nameOfFile += ".phtml";
            else
                nameOfFile += ".php";

            FileStream fileStream;

            if (Array.IndexOf((new string[] { "Layout", "Component" }), typeOfFile) != -1)
                fileStream = new FileStream(Path.Combine("..", "Application", "Views", String.Format("{0}s", correctTypeName), nameOfFile), FileMode.CreateNew);
            else
                fileStream = new FileStream(Path.Combine("..", "Application", String.Format("{0}s", correctTypeName), nameOfFile), FileMode.CreateNew);

            using (StreamWriter streamWriter = new StreamWriter(fileStream))
            {
                streamWriter.Write(content);
                streamWriter.Flush();
            }
        }

        public void EditSettingsInConfigurationFile(ConfigurationFile configurationFile, Dictionary<string, string> Settings)
        {
            string filePath = "";
            string settingsArrayName = "";

            if (configurationFile == ConfigurationFile.ApplicationConfiguration)
            {
                filePath = Path.Combine("..", "Application", "Configurations", "ApplicationConfiguration.php");
                settingsArrayName = "applicationSettings";
            }
            else if (configurationFile == ConfigurationFile.DatabaseConfiguration)
            {
                filePath = Path.Combine("..", "Application", "Configurations", "DatabaseConfiguration.php");
                settingsArrayName = "databaseSettings";
            }

            //Reading a file
            string fileContent = "";

            FileStream readingFileStream = new FileStream(filePath, FileMode.Open);

            using (StreamReader streamReader = new StreamReader(readingFileStream))
            {
                fileContent = streamReader.ReadToEnd();
                readingFileStream.Flush();
            }

            //Overwriting a file
            FileStream writingFileStream = new FileStream(filePath, FileMode.Create);

            using (StreamWriter streamWriter = new StreamWriter(writingFileStream))
            {
                foreach (KeyValuePair<string, string> setting in Settings)
                {
                    string definitionWithoutEqualsSign = String.Format("$this->{0}['{1}']", settingsArrayName, setting.Key);

                    string firstPartOfSettingDefinition = String.Format(
                            "$this->{0}['{1}']{2}",
                            settingsArrayName,
                            setting.Key,
                            fileContent.Substring(
                                fileContent.IndexOf(definitionWithoutEqualsSign) + definitionWithoutEqualsSign.Length,
                                FindEndIndexWithDefinedStart(
                                        fileContent,
                                        fileContent.IndexOf(definitionWithoutEqualsSign) + definitionWithoutEqualsSign.Length,
                                        "'"
                                    ) - (fileContent.IndexOf(definitionWithoutEqualsSign) + definitionWithoutEqualsSign.Length)
                                )
                        );

                    fileContent = fileContent.Replace(
                            String.Format(
                                    "{0}{1}",
                                    firstPartOfSettingDefinition,
                                    fileContent.Substring(
                                            fileContent.IndexOf(firstPartOfSettingDefinition) + firstPartOfSettingDefinition.Length,
                                            FindEndIndexWithDefinedStart(
                                                    fileContent,
                                                    fileContent.IndexOf(firstPartOfSettingDefinition) + firstPartOfSettingDefinition.Length,
                                                    "';"
                                                ) - (fileContent.IndexOf(firstPartOfSettingDefinition) + firstPartOfSettingDefinition.Length)
                                        )
                                ),
                            String.Format("{0}'{1}", firstPartOfSettingDefinition, setting.Value)
                        );
                }

                streamWriter.Write(fileContent);
                streamWriter.Flush();
            }
        }

        public void EditSettingsInConfigurationFile(ConfigurationFile configurationFile, Dictionary<string, bool> Settings)
        {
            string filePath = "";
            string settingsArrayName = "";

            if (configurationFile == ConfigurationFile.ApplicationConfiguration)
            {
                filePath = Path.Combine("..", "Application", "Configurations", "ApplicationConfiguration.php");
                settingsArrayName = "applicationSettings";
            }
            else if (configurationFile == ConfigurationFile.DatabaseConfiguration)
            {
                filePath = Path.Combine("..", "Application", "Configurations", "DatabaseConfiguration.php");
                settingsArrayName = "databaseSettings";
            }

            //Reading a file
            string fileContent = "";

            FileStream readingFileStream = new FileStream(filePath, FileMode.Open);

            using (StreamReader streamReader = new StreamReader(readingFileStream))
            {
                fileContent = streamReader.ReadToEnd();
                readingFileStream.Flush();
            }

            //Overwriting a file
            FileStream writingFileStream = new FileStream(filePath, FileMode.Create);

            using (StreamWriter streamWriter = new StreamWriter(writingFileStream))
            {
                foreach (KeyValuePair<string, bool> setting in Settings)
                {
                    string definitionWithoutEqualsSign = String.Format("$this->{0}['{1}']", settingsArrayName, setting.Key);

                    string firstPartOfSettingDefinition = String.Format(
                            "$this->{0}['{1}']{2}",
                            settingsArrayName,
                            setting.Key,
                            fileContent.Substring(
                                    fileContent.IndexOf(definitionWithoutEqualsSign) + definitionWithoutEqualsSign.Length,
                                    fileContent.Substring(fileContent.IndexOf(definitionWithoutEqualsSign) + definitionWithoutEqualsSign.Length).TakeWhile(c => c == ' ' || c == '=').Count()
                                )
                        );

                    fileContent = fileContent.Replace(
                            String.Format(
                                    "{0}{1}",
                                    firstPartOfSettingDefinition,
                                    fileContent.Substring(
                                            fileContent.IndexOf(firstPartOfSettingDefinition) + firstPartOfSettingDefinition.Length,
                                            FindEndIndexWithDefinedStart(
                                                    fileContent,
                                                    fileContent.IndexOf(firstPartOfSettingDefinition) + firstPartOfSettingDefinition.Length,
                                                    ";"
                                                ) - (fileContent.IndexOf(firstPartOfSettingDefinition) + firstPartOfSettingDefinition.Length)
                                        )
                                ),
                            String.Format("{0}{1}", firstPartOfSettingDefinition, setting.Value.ToString().ToLower())
                        );
                }

                streamWriter.Write(fileContent);
                streamWriter.Flush();
            }
        }

        public void AddNewSettingsToConfigurationFile(ConfigurationFile configurationFile, Dictionary<string, string> Settings)
        {
            string filePath = "";
            string settingsArrayName = "";

            if (configurationFile == ConfigurationFile.ApplicationConfiguration)
            {
                filePath = Path.Combine("..", "Application", "Configurations", "ApplicationConfiguration.php");
                settingsArrayName = "applicationSettings";
            }
            else if (configurationFile == ConfigurationFile.DatabaseConfiguration)
            {
                filePath = Path.Combine("..", "Application", "Configurations", "DatabaseConfiguration.php");
                settingsArrayName = "databaseSettings";
            }
            else
            {
                filePath = Path.Combine("..", "Application", "Configurations", "RoutingTableConfiguration.php");
                settingsArrayName = "routingTable";
            }

            //Reading a file
            string fileContent = "";

            FileStream readingFileStream = new FileStream(filePath, FileMode.Open);

            using (StreamReader streamReader = new StreamReader(readingFileStream))
            {
                fileContent = streamReader.ReadToEnd();
                readingFileStream.Flush();
            }

            //Overwriting a file
            FileStream writingFileStream = new FileStream(filePath, FileMode.Create);

            using (StreamWriter streamWriter = new StreamWriter(writingFileStream))
            {
                foreach (KeyValuePair<string, string> setting in Settings)
                {
                    string definition = String.Format("\n\t\t$this->{0}['{1}'] = '{2}';", settingsArrayName, setting.Key, setting.Value);

                    fileContent = fileContent.Insert(fileContent.LastIndexOf(';') + 1, definition);
                }

                streamWriter.Write(fileContent);
                streamWriter.Flush();
            }
        }


        public string[] GetListOfDBTemplates()
        {
            return Directory.GetFiles(Path.Combine("..", "Application", "DBTemplates"));
        }

        public bool CheckExistenceOfMigration(string migrationName)
        {
            return File.Exists(Path.Combine("..", "Application", "DBTemplates", String.Format("{0}.php", migrationName)));
        }

        private int FindEndIndexWithDefinedStart(string text, int startIndex, string endPiece)
        {
            int iterator = 0;

            while (text.Substring(startIndex + iterator, endPiece.Length) != endPiece)
                iterator++;

            return startIndex + iterator;
        }
    }
}
