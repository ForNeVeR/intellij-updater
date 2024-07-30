namespace IntelliJUpdater

open System.IO
open System.Text.RegularExpressions
open System.Threading.Tasks
open TruePath

// TODO: All these methods won't properly handle the first line in the file.

module TomlFile =
    let ReadValue (tomlFile: LocalPath) (key: string): Task<string> = task {
        let! toml = File.ReadAllTextAsync tomlFile.Value
        let re = Regex $@"[\r\n]{Regex.Escape key} = ""(.*?)"""
        let matches = re.Match(toml)
        if not matches.Success then failwithf $"Cannot find the key \"{key}\" in the TOML file \"{tomlFile}\"."
        return matches.Groups[1].Value
    }

    let WriteValue (tomlFile: LocalPath) (key: string) (value: string): Task<bool> = task {
        let! toml = File.ReadAllTextAsync tomlFile.Value
        let re = Regex $@"[\r\n]{Regex.Escape key} = ""(.*?)"""
        let newContent = re.Replace(toml, $"\n{key} = \"{value}\"")
        do! File.WriteAllTextAsync(tomlFile.Value, newContent)
        return toml <> newContent
    }


module PropertiesFile =
    let ReadValue (propertiesFile: LocalPath) (key: string) = task {
        let! properties = File.ReadAllTextAsync propertiesFile.Value
        let re = Regex $@"\r?\n{Regex.Escape key}=(.*?)\r?\n"
        let matches = re.Match(properties)
        if not matches.Success then failwithf $"Cannot find the key \"{key}\" in the properties file \"{propertiesFile}\"."
        return matches.Groups[1].Value
    }

    let WriteValue (propertiesFile: LocalPath) (key: string) (value: string): Task<bool> = task {
        let! properties = File.ReadAllTextAsync propertiesFile.Value
        let re = Regex @"\r?\n{Regex.Escape key}=(.*?)\r?\n"
        let newContent = re.Replace(properties, $"\n{key}={value}\n")
        do! File.WriteAllTextAsync(propertiesFile.Value, newContent)
        return properties <> newContent
    }
