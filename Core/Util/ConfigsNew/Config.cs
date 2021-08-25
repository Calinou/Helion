﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Helion.Input;
using Helion.Util.ConfigsNew.Components;
using Helion.Util.ConfigsNew.Values;
using IniParser;
using IniParser.Model;
using NLog;

namespace Helion.Util.ConfigsNew
{
    public record ConfigComponent(string Path, ConfigInfoAttribute Attribute, IConfigValue Value);

    public class ConfigNew : IEnumerable<ConfigComponent>
    {
        private const string EngineSectionName = "engine";
        private const string KeysSectionName = "keys";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public readonly ConfigPlayer Player = new();
        public readonly ConfigKeyMapping Keys = new();
        private readonly Dictionary<string, ConfigComponent> m_components = new(StringComparer.OrdinalIgnoreCase);
        private readonly string? m_path;
            
        public ConfigNew()
        {
            PopulateComponentsRecursively(this, "");
        }

        public ConfigNew(string path) : this()
        {
            m_path = path;

            ReadConfigFrom(path);
        }

        public void ApplyQueuedChanges(ConfigSetFlags setFlags)
        {
            Log.ConditionalTrace("Applying queued config changes for {Flags}", setFlags);
            
            foreach (ConfigComponent component in m_components.Values)
                component.Value.ApplyQueuedChange(setFlags);
        }

        public bool Write(string? filePath = null)
        {
            filePath ??= m_path;

            try
            {
                FileIniDataParser parser = new();
                IniData iniData = new();

                bool success = true;
                success &= AddEngineFields(iniData);
                success &= AddKeyFields(iniData);

                if (success)
                    parser.WriteFile(filePath, iniData);

                return success;
            }
            catch
            {
                return false;
            }

            bool AddEngineFields(IniData data)
            {
                if (!data.Sections.AddSection(EngineSectionName))
                {
                    Log.Error("Failed to add engine section header when writing config");
                    return false;
                }

                KeyDataCollection section = data[EngineSectionName];
                foreach ((string entryPath, _, IConfigValue configValue) in m_components.Values)
                    section[entryPath] = configValue.ToString();

                return true;
            }
            
            bool AddKeyFields(IniData data)
            {
                if (!data.Sections.AddSection(KeysSectionName))
                {
                    Log.Error("Failed to add key section header when writing config");
                    return false;
                }

                KeyDataCollection section = data[KeysSectionName];
                foreach ((Key key, IEnumerable<string> commands) in Keys)
                {
                    IEnumerable<string> quotedCommands = commands.Select(c => $"\"{c}\"");
                    section[key.ToString()] = $"[{string.Join(", ", quotedCommands)}]";
                }

                return true;
            }
        }

        private void PopulateComponentsRecursively(object obj, string path)
        {
            foreach (FieldInfo fieldInfo in obj.GetType().GetFields())
            {
                if (!fieldInfo.IsPublic)
                    continue;

                object? childObj = fieldInfo.GetValue(obj);
                if (childObj == null)
                    throw new Exception($"Missing config object instantiation {fieldInfo.Name} at '{path}'");

                string newPath = (path != "" ? $"{path}." : "") + fieldInfo.Name.ToLower();

                if (childObj is IConfigValue configValue)
                {
                    ConfigInfoAttribute? attribute = fieldInfo.GetCustomAttribute<ConfigInfoAttribute>();
                    if (attribute == null)
                        throw new Exception($"Config field at '{newPath}' is missing attribute {nameof(ConfigInfoAttribute)}");
                    
                    m_components[newPath] = new ConfigComponent(newPath, attribute, configValue);
                    continue;
                }

                PopulateComponentsRecursively(childObj, newPath);
            }
        }

        private void ReadConfigFrom(string path)
        {
            Log.Debug("Reading config file from {Path}", path);
            
            if (!File.Exists(path))
            {
                Log.Info($"Config file not found, will generate new config file at {path}");
                return;
            }

            try
            {
                FileIniDataParser parser = new();
                IniData iniData = parser.ReadFile(path);
                foreach (KeyData keyData in iniData.Sections[EngineSectionName])
                {
                    string identifier = keyData.KeyName.ToLower();

                    if (!m_components.TryGetValue(identifier, out ConfigComponent? configComponent))
                    {
                        Log.Warn($"Unable to find config mapping for {identifier}, value will be lost on saving");
                        continue;
                    }

                    if (!configComponent.Attribute.Save)
                    {
                        Log.Warn($"Config mapping {identifier} is transient (and ignored), value will be excluded on saving");
                        continue;
                    }

                    configComponent.Value.Set(keyData.Value);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Unable to parse config file: {e.Message}");
            }
        }

        public IEnumerator<ConfigComponent> GetEnumerator() => m_components.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
