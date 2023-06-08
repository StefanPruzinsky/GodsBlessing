using System;
using System.Collections.Generic;

using GodsBlessing.DatabaseLibrary;
using Newtonsoft.Json.Linq;

namespace GodsBlessing
{
    class TerminalHelper
    {
        public DatabaseManager DatabaseManager { get; private set; }
        public PasswordGenerator PasswordGenerator { get; private set; }
        public APIHelper APIHelper { get; private set; }
        public FileSystemHelper FileSystemHelper { get; private set; }
        public StringHelper StringHelper { get; private set; }

        public TerminalHelper(DatabaseManager databaseManager)
        {
            DatabaseManager = databaseManager;
            PasswordGenerator = new PasswordGenerator();

            if (DatabaseManager.InitializationExecution)
                APIHelper = new APIHelper(DatabaseManager.APIKey, DatabaseManager.ServerPath);

            FileSystemHelper = new FileSystemHelper();
            StringHelper = new StringHelper();
        }

        public void InitializeApp()
        {
            //Dictionary for editing settings
            Dictionary<string, string> settings = new Dictionary<string, string>();

            //API key
            string apiKey = PasswordGenerator.CreateSimplePassword(15);

            settings.Add("apiKey", apiKey);
            FileSystemHelper.EditSettingsInConfigurationFile(ConfigurationFile.ApplicationConfiguration, settings);

            DatabaseManager.SetGodsBlessingSetting("apiKey", apiKey);

            //Initialization setup
            WriteLineHeader("Welcome to 1st initialization of your application!");
            Console.WriteLine("In the following steps we will set all neccessary things for successful development of your app.");

            //Application settings
            WriteLineHeader("Application settings:\n---------------------");

            Console.Write("Application name: ");
            string applicationName = Console.ReadLine().Trim();

            Console.Write("Server address (http://example.com): ");
            string serverAddress = (serverAddress = Console.ReadLine()).TrimEnd(new char[]{ '/', '\\'});

            Console.Write("Path to website directory (directory1/directory2/directory3, if none leave empty): ");
            string pathToWebsiteDirectory = (pathToWebsiteDirectory = Console.ReadLine()).Trim(new char[] { '/', '\\' });

            while (!(APIHelper = new APIHelper(apiKey, String.Format("{0}/{1}/", serverAddress, pathToWebsiteDirectory))).TestConnection())
            {
                WriteLineNotificationMessage("Bad server address or path to website directory was entered. Please enter correct data again:");

                Console.Write("Server address (http://example.com): ");
                serverAddress = (serverAddress = Console.ReadLine()).TrimEnd(new char[] { '/', '\\' });

                Console.Write("Path to website directory (directory1/directory2/directory3, if none leave empty): ");
                pathToWebsiteDirectory = (pathToWebsiteDirectory = Console.ReadLine()).Trim(new char[] { '/', '\\' });
            }

            //Applying application settings
            settings.Clear();

            settings.Add("applicationName", applicationName);
            settings.Add("serverAddress", serverAddress);
            settings.Add("pathToWebsiteDirectory", pathToWebsiteDirectory);

            FileSystemHelper.EditSettingsInConfigurationFile(ConfigurationFile.ApplicationConfiguration, settings);

            //Updating settings in database
            DatabaseManager.SetGodsBlessingSetting("wasInitialized", "True");
            DatabaseManager.SetGodsBlessingSetting("serverPath", String.Format("{0}/{1}", serverAddress, pathToWebsiteDirectory));

            WriteLineSuccessMessage("Application has been setup successfuly!\n");

            //Database settings
            SetUpDatabase();

            WriteLineSuccessMessage("\nYou have just passed application setup. It's time to start developing your app! God bless you! :)");
        }

        public void DeployApp()
        {
            //Dictionary for editing settings
            Dictionary<string, string> settings = new Dictionary<string, string>();
            Dictionary<string, bool> otherSettings = new Dictionary<string, bool>();

            //Initialization setup
            WriteLineHeader("Welcome to deploying of your application!");
            Console.WriteLine("In the following steps we will set all neccessary things for successful deployment of your app.");

            //Application settings
            Console.WriteLine("Application settings:\n---------------------");

            Console.Write("Server address (http://example.com): ");
            string serverAddress = (serverAddress = Console.ReadLine()).TrimEnd(new char[] { '/', '\\' });

            //Applying application settings
            settings.Clear();

            settings.Add("serverAddress", serverAddress);
            otherSettings.Add("debugMode", false);

            FileSystemHelper.EditSettingsInConfigurationFile(ConfigurationFile.ApplicationConfiguration, settings);
            FileSystemHelper.EditSettingsInConfigurationFile(ConfigurationFile.ApplicationConfiguration, otherSettings);

            WriteLineSuccessMessage("Application has been setup successfuly!\n");

            //Database settings
            SetUpDatabase();

            WriteLineHeader("It's s time to deploy the app into a real production! God bless you! :) (Applicaton *.zip archive is located in root of your website directory.)");
        }

        public bool Migrate(string migrationType)
        {
            Dictionary<string, dynamic> templates = new Dictionary<string, dynamic>();

            if (migrationType == "all")
            {
                foreach (string DBTemplate in FileSystemHelper.GetListOfDBTemplates())
                {
                    string nameOfDBTemplate = DBTemplate.Remove(0, DBTemplate.LastIndexOf('\\') + 1);

                    templates.Add(nameOfDBTemplate.Replace("DBTemplate.php", "").ToLower(), APIHelper.GetData(nameOfDBTemplate.Replace(".php", ""), "dbTemplate").response);
                }
            }
            else
            {
                if (FileSystemHelper.CheckExistenceOfMigration(migrationType.Contains("DBTemplate") ? migrationType.Trim() : String.Format("{0}DBTemplate", migrationType).Trim()))
                {
                    string tableName = "";

                    if (migrationType.Contains("DBTemplate"))
                        tableName = migrationType.Replace("DBTemplate", "").ToLower().Trim();
                    else
                        tableName = migrationType.ToLower().Trim();

                    string className = "";

                    if (migrationType.Contains("DBTemplate"))
                        className = StringHelper.FirstLetterToUpper(migrationType.Trim());
                    else
                        className = StringHelper.FirstLetterToUpper(String.Format("{0}DBTemplate", migrationType).Trim());

                    dynamic dbTemplate = APIHelper.GetData(className, "dbTemplate").response;

                    templates.Add(tableName, dbTemplate);
                }
                else
                {
                    WriteLineErrorMessage(String.Format("Migration {0} doesn't exist.", migrationType));
                    return false;
                }
            }

            bool wasSuccessful = false;

            try
            {
                foreach (KeyValuePair<string, dynamic> template in templates)
                    DatabaseManager.CreateTable(template.Key, template.Value);

                WriteSuccessMessage("Database has migrated successfuly!");

                wasSuccessful = true;
            }
            catch
            {
                WriteErrorMessage("Database migration failed! Please try to check the state of a database server or edit your database access data.");
            }

            return wasSuccessful;
        }

        public void Create(string[] options)
        {
            string[] scriptTypes = { "administration", "archivist", "helper", "controller", "object", "layout", "view", "component", "dbtemplate" };

            string typeOfScriptToCreate = options[0].ToLower();

            if (Array.IndexOf(scriptTypes, typeOfScriptToCreate) == -1)
            {
                WriteNotificationMessage(String.Format("Script type {0} doesn't exist.", options[0]));
                return;
            }

            Dictionary<string, int> keyValueReplacingPairs = new Dictionary<string, int>();

            if (typeOfScriptToCreate == "administration")
                CreateAdministration();
            else if (typeOfScriptToCreate == "archivist")
            {
                keyValueReplacingPairs.Add("##name##", 0);
                keyValueReplacingPairs.Add("##table name##", 2);

                CreateScript(options, 3, keyValueReplacingPairs, "Archivist");
            }
            else if (typeOfScriptToCreate == "helper")
            {
                keyValueReplacingPairs.Add("##name##", 0);

                CreateScript(options, 2, keyValueReplacingPairs, "Helper");
            }
            else if (typeOfScriptToCreate == "controller")
            {
                keyValueReplacingPairs.Add("##name##", 0);
                keyValueReplacingPairs.Add("##view##", 2);
                keyValueReplacingPairs.Add("##layout##", 3);

                CreateScript(options, 4, keyValueReplacingPairs, "Controller");
            }
            else if (typeOfScriptToCreate == "Object")
            {
                keyValueReplacingPairs.Add("##name##", 0);

                CreateScript(options, 2, keyValueReplacingPairs, "Object");
            }
            else if (typeOfScriptToCreate == "layout")
                CreateScript(options, 2, keyValueReplacingPairs, "Layout");
            else if (typeOfScriptToCreate == "view")
                CreateScript(options, 2, keyValueReplacingPairs, "View");
            else if (typeOfScriptToCreate == "component")
                CreateScript(options, 2, keyValueReplacingPairs, "Component");
            else if (typeOfScriptToCreate == "dbtemplate")
            {
                keyValueReplacingPairs.Add("##name##", 0);

                CreateScript(options, 2, keyValueReplacingPairs, "DBTemplate");
            }
        }

        private void SetUpDatabase()
        {
            Console.WriteLine("Database settings:\n------------------");

            Console.Write("Host name: ");
            string hostName = Console.ReadLine().Trim();

            Console.Write("Database name: ");
            string databaseName = Console.ReadLine().Trim();

            Console.Write("User name: ");
            string userName = Console.ReadLine().Trim();

            Console.Write("Password (can't be empty): ");
            string password = Console.ReadLine().Trim();

            //Applying settings
            Dictionary<string, string> settings = new Dictionary<string, string>();

            settings.Add("hostName", hostName);
            settings.Add("databaseName", databaseName);
            settings.Add("userName", userName);
            settings.Add("password", password);

            FileSystemHelper.EditSettingsInConfigurationFile(ConfigurationFile.DatabaseConfiguration, settings);

            while (!DatabaseManager.TryDatabaseConnection(DatabaseType.MySQL))
            {
                WriteLineNotificationMessage("\nDatabase connection wasn't successful. Please enter correct data again:");

                Console.Write("Host name: ");
                hostName = Console.ReadLine().Trim();

                Console.Write("Database name: ");
                databaseName = Console.ReadLine().Trim();

                Console.Write("User name: ");
                userName = Console.ReadLine().Trim();

                Console.Write("Password (can't be empty): ");
                password = Console.ReadLine().Trim();

                //Applying settings
                settings.Clear();

                settings.Add("hostName", hostName);
                settings.Add("databaseName", databaseName);
                settings.Add("userName", userName);
                settings.Add("password", password);

                FileSystemHelper.EditSettingsInConfigurationFile(ConfigurationFile.DatabaseConfiguration, settings);
            }

            WriteLineSuccessMessage("The database has been set up successfuly!");
        }

        private void CreateAdministration()
        {
            if (!FileSystemHelper.CheckExistenceOfMigration("UsersDBTemplate"))
            {
                WriteErrorMessage("UsersDBTemplate doesn't exist! Please, create it or edit template name required way.");
                return;
            }

            WriteNotificationMessage("First of all, make sure, you have edited UsersDBTemplate according to your specific purposes. (press any key to continue)");
            Console.ReadKey();

            //Migrating of users table
            if (!Migrate("UsersDBTemplate"))
                return;

            //Admin creation
            Console.WriteLine("\nAdmin creation:\n---------------");

            Dictionary<string, string> adminData = new Dictionary<string, string>();
            dynamic tableTemplate = APIHelper.GetData("UsersDBTemplate", "dbTemplate").response;

            foreach (JProperty columnDefinition in tableTemplate)
            {
                if (Array.IndexOf(new string[] { "id", "timeOfCreation", "role" }, columnDefinition.Name) == -1)
                {
                    Console.Write(String.Format("{0}: ", StringHelper.FirstLetterToUpper(columnDefinition.Name)));

                    if (columnDefinition.Name.ToLower() == "password") {
                        adminData.Add(columnDefinition.Name, APIHelper.GetData("GodsBlessingHelper", "HashPassword", new string[] { Console.ReadLine().Trim() }).response.ToString());
                    }
                    else
                        adminData.Add(columnDefinition.Name, Console.ReadLine().Trim());
                }
            }

            adminData.Add("role", "admin");

            DatabaseManager.CreateNewUser(adminData);

            WriteLineSuccessMessage("Admin was successfuly created!\n");

            //Creating files needed for users management system.
            Console.Write("Do you want to allow new users to registrate? (Y/N): ");

            char result = ' ';

            while (Array.IndexOf((new char[] { 'Y', 'N' }), (result = Console.ReadKey().KeyChar)) == -1)
                WriteNotificationMessage("You have choosed incorrect option, please try again (Y/N): ");

            try
            {
                if (result == 'Y')
                {
                    string content = "";

                    content = DatabaseManager.GetUserManagementScript("signingController");
                    FileSystemHelper.CreateFile("Controller", "SigningController", content);

                    content = DatabaseManager.GetUserManagementScript("userController");
                    FileSystemHelper.CreateFile("Controller", "UserController", content);

                    content = DatabaseManager.GetUserManagementScript("adminController");
                    FileSystemHelper.CreateFile("Controller", "AdminController", content);

                    content = DatabaseManager.GetUserManagementScript("signingHelper");
                    FileSystemHelper.CreateFile("Helper", "SigningHelper", content);

                    content = DatabaseManager.GetUserManagementScript("signingView");
                    FileSystemHelper.CreateFile("View", "signingView", content);

                    content = DatabaseManager.GetUserManagementScript("profileView");
                    FileSystemHelper.CreateFile("View", "profileView", content);

                    content = DatabaseManager.GetUserManagementScript("administrationView");
                    FileSystemHelper.CreateFile("View", "administrationView", content);

                    content = DatabaseManager.GetUserManagementScript("userArchivist");
                    FileSystemHelper.CreateFile("Archivist", "UserArchivist", content);

                    Dictionary<string, string> settings = new Dictionary<string, string>();

                    settings.Add("login", "SigningController");
                    settings.Add("register", "SigningController");
                    settings.Add("profile", "UserController");
                    settings.Add("administration", "AdminController");

                    FileSystemHelper.AddNewSettingsToConfigurationFile(ConfigurationFile.RoutingTableConfiguration, settings);
                }
                else
                {
                    string content = "";

                    content = DatabaseManager.GetUserManagementScript("onlyAdmin_signingController");
                    FileSystemHelper.CreateFile("Controller", "SigningController", content);

                    content = DatabaseManager.GetUserManagementScript("onlyAdmin_adminController");
                    FileSystemHelper.CreateFile("Controller", "AdminController", content);

                    content = DatabaseManager.GetUserManagementScript("onlyAdmin_signingHelper");
                    FileSystemHelper.CreateFile("Helper", "SigningHelper", content);

                    content = DatabaseManager.GetUserManagementScript("onlyAdmin_signingView");
                    FileSystemHelper.CreateFile("View", "signingView", content);

                    content = DatabaseManager.GetUserManagementScript("onlyAdmin_administrationView");
                    FileSystemHelper.CreateFile("View", "administrationView", content);

                    content = DatabaseManager.GetUserManagementScript("onlyAdmin_userArchivist");
                    FileSystemHelper.CreateFile("Archivist", "UserArchivist", content);

                    Dictionary<string, string> settings = new Dictionary<string, string>();

                    settings.Add("login", "SigningController");
                    settings.Add("administration", "AdminController");

                    FileSystemHelper.AddNewSettingsToConfigurationFile(ConfigurationFile.RoutingTableConfiguration, settings);
                }

                WriteErrorMessage("User management system creating failed, please check possible errors and try again.");
            }
            catch
            {
                WriteSuccessMessage("User management system was successfuly created!");
            }
        }

        private void CreateScript(string[] options, int requiredNumberOfOptions, Dictionary<string, int> keyValueReplacingPairs, string typeOfScriptToCreate)
        {
            if (options.Length != requiredNumberOfOptions)
            {
                WriteNotificationMessage("Incorrect number of parameters. Please, try again.");
                return;
            }

            string nameOfScriptToCreate = options[1];

            if (nameOfScriptToCreate.ToLower().Contains(typeOfScriptToCreate.ToLower()))
            {
                if (Array.IndexOf((new string[] { "View", "Component" }), typeOfScriptToCreate) != -1)
                    nameOfScriptToCreate = String.Format("{0}{1}", StringHelper.FirstLetterToLower(nameOfScriptToCreate.Substring(0, nameOfScriptToCreate.ToLower().IndexOf(typeOfScriptToCreate.ToLower()))), typeOfScriptToCreate);
                else
                    nameOfScriptToCreate = String.Format("{0}{1}", StringHelper.FirstLetterToUpper(nameOfScriptToCreate.Substring(0, nameOfScriptToCreate.ToLower().IndexOf(typeOfScriptToCreate.ToLower()))), typeOfScriptToCreate);
            }
            else
            {
                if (Array.IndexOf((new string[] { "View", "Component" }), typeOfScriptToCreate) != -1)
                    nameOfScriptToCreate = String.Format("{0}{1}", StringHelper.FirstLetterToLower(nameOfScriptToCreate), typeOfScriptToCreate);
                else
                    nameOfScriptToCreate = String.Format("{0}{1}", StringHelper.FirstLetterToUpper(nameOfScriptToCreate), typeOfScriptToCreate);
            }

            try
            {
                string script = DatabaseManager.GetStuffScript(typeOfScriptToCreate.ToLower());

                foreach (KeyValuePair<string, int> keyValueReplacingPair in keyValueReplacingPairs)
                    if (keyValueReplacingPair.Value == 0)
                        script = script.Replace(keyValueReplacingPair.Key, nameOfScriptToCreate);
                    else if (keyValueReplacingPair.Value == -1)
                        script = StringHelper.ConvertFromCamelCaseToUnderscoreCase(script.Replace(keyValueReplacingPair.Key, nameOfScriptToCreate.Replace(typeOfScriptToCreate, "")));
                    else
                        script = script.Replace(keyValueReplacingPair.Key, options[keyValueReplacingPair.Value]);

                FileSystemHelper.CreateFile(typeOfScriptToCreate, nameOfScriptToCreate, script);

                WriteSuccessMessage(String.Format("{0} was created successfuly!", nameOfScriptToCreate));
            }
            catch
            {
                WriteErrorMessage(String.Format("{0} creating failed! Please try again.", typeOfScriptToCreate));
            } 
        }

        public void WriteSuccessMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            Console.Write("\n{0}", message);

            Console.ResetColor();
        }

        public void WriteErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.Write("\n{0}", message);

            Console.ResetColor();
        }

        public void WriteHeaderMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;

            Console.Write("\n{0}", message);

            Console.ResetColor();
        }

        public void WriteNotificationMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;

            Console.Write("\n{0}", message);

            Console.ResetColor();
        }

        public void WriteLineSuccessMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("{0}", message);

            Console.ResetColor();
        }

        public void WriteLineErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("{0}", message);

            Console.ResetColor();
        }

        public void WriteLineHeader(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;

            Console.WriteLine("{0}", message);

            Console.ResetColor();
        }

        public void WriteLineNotificationMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;

            Console.WriteLine("{0}", message);

            Console.ResetColor();
        }
    }
}
