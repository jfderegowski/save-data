using System;
using System.IO;
using System.Threading.Tasks;
using fefek5.Variables.SaveDataVariable.Runtime;
using fefek5.Variables.SaveDataVariable.Runtime.Settings;
using fefek5.Variables.SerializableGuidVariable.Runtime;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Tests
{
    public class SaveDataTests
    {
        private SaveData _saveData;
        private string _testFilePath;

        [SetUp]
        public void SetUp()
        {
            _saveData = new SaveData();
            _testFilePath = Path.Combine(Application.persistentDataPath, "test_save_data.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }

        [Test]
        public void SetKeyAndGetKeyByStringWorksCorrectly()
        {
            const string key = "testKey";
            const int value = 42;

            _saveData.SetKey(key, value);
            var result = _saveData.GetKey(key, 0);

            Assert.AreEqual(value, result);
        }

        [Test]
        public void SetKeyAndGetKeyByGuidWorksCorrectly()
        {
            var key = Guid.NewGuid();
            const string value = "Hello";

            _saveData.SetKey(key, value);
            var result = _saveData.GetKey(key, string.Empty);

            Assert.AreEqual(value, result);
        }

        [Test]
        public void SetKeyAndGetKeyBySerializableGuidWorksCorrectly()
        {
            var key = new SerializableGuid(Guid.NewGuid());
            const float value = 3.14f;

            _saveData.SetKey(key, value);
            var result = _saveData.GetKey(key, 0f);

            Assert.AreEqual(value, result);
        }

        [Test]
        public void SetKeyAndGetKeyBySaveKeyWorksCorrectly()
        {
            var saveKey = new SaveKey(Guid.NewGuid(), "customKey");
            const bool value = true;

            _saveData.SetKey(saveKey, value);
            var result = _saveData.GetKey(saveKey, false);

            Assert.AreEqual(value, result);
        }

        [Test]
        public void TryGetKeyReturnsTrueWhenKeyExists()
        {
            const string key = "existingKey";
            const int expected = 100;
            _saveData.SetKey(key, expected);

            var found = _saveData.TryGetKey(key, out int result);

            Assert.IsTrue(found);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TryGetKeyReturnsFalseWhenKeyDoesNotExist()
        {
            var found = _saveData.TryGetKey("missingKey", out int result);

            Assert.IsFalse(found);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void RemoveKeyRemovesKeySuccessfully()
        {
            const string key = "keyToRemove";
            _saveData.SetKey(key, 999);

            _saveData.RemoveKey(key);

            var exists = _saveData.IsKeyExist(key);
            Assert.IsFalse(exists);
        }

        [Test]
        public void IsKeyExistReturnsCorrectly()
        {
            const string key = "someKey";
            _saveData.SetKey(key, "value");

            Assert.IsTrue(_saveData.IsKeyExist(key));
            Assert.IsFalse(_saveData.IsKeyExist("nonExistingKey"));
        }

        [Test]
        public void IndexerWorksForStringKey()
        {
            const string key = "indexerKey";
            const int value = 12345;

            _saveData[key] = value;
            var result = _saveData[key];

            Assert.AreEqual(value, result);
        }

        [Test]
        public void SaveDataCanBeSerializedToJsonAndDeserialized()
        {
            _saveData.SetKey("score", 999);
            _saveData.SetKey("playerName", "TestPlayer");

            var json = _saveData.ToJson();
            var loadedData = new SaveData().FromJson(json);

            var score = loadedData.GetKey("score", 0);
            var playerName = loadedData.GetKey("playerName", "");

            Assert.AreEqual(999, score);
            Assert.AreEqual("TestPlayer", playerName);
        }

        [Test]
        public async Task SaveDataCanBeSavedAndLoadedFromDisk()
        {
            _saveData.SetKey("level", 5);
            _saveData.Save(_testFilePath);

            // Wait for async save to complete
            await Task.Delay(500);

            var loadedSaveData = new SaveData();
            loadedSaveData.Load(_testFilePath);

            await Task.Delay(500); // wait for async load

            var level = loadedSaveData.GetKey("level", 0);

            Assert.AreEqual(5, level);
        }

        [Test]
        public void SettingNullValueDoesNotThrow()
        {
            Assert.DoesNotThrow(() => _saveData.SetKey("nullKey", null));
        }

        [Test]
        public void OverwritingValueUpdatesCorrectly()
        {
            const string key = "overwriteKey";
            _saveData.SetKey(key, 1);
            _saveData.SetKey(key, 2);

            var result = _saveData.GetKey(key, 0);

            Assert.AreEqual(2, result);
        }

        [Test]
        public void RemovingNonExistingKeyDoesNotThrow()
        {
            Assert.DoesNotThrow(() => _saveData.RemoveKey("nonExistingKey"));
        }

        [Test]
        public void GettingMissingKeyReturnsDefault()
        {
            const int defaultValue = 999;
            var result = _saveData.GetKey("missingKey", defaultValue);

            Assert.AreEqual(defaultValue, result);
        }

        [Test]
        public void SaveDataWithCustomSaveSettingsSerializesCorrectly()
        {
            var settings = new SaveSettings {
                UseJsonCustomSettings = true,
                JsonCustomSettings = new JsonSettings {
                    Formatting = Formatting.Indented
                }
            };

            _saveData.SetKey("test", 123);
            var json = _saveData.ToJson(settings);

            Assert.IsTrue(json.Contains("\n")); // pretty-printed JSON should have new lines
        }

        [Test]
        public void SaveDataDeleteExcessFilesWorks()
        {
            var folder = Path.Combine(Application.persistentDataPath, "test_folder");
            Directory.CreateDirectory(folder);

            for (var i = 0; i < 5; i++) 
                File.WriteAllText(Path.Combine(folder, $"file_{i}.sav"), "dummy data");

            SaveData.DeleteExcessFiles(folder, "*.sav", 2);

            var files = Directory.GetFiles(folder, "*.sav");
            Assert.AreEqual(2, files.Length);

            Directory.Delete(folder, true);
        }

        [Test]
        public void SaveDataGetFilesReturnsCorrectFiles()
        {
            var folder = Path.Combine(Application.persistentDataPath, "getfiles_folder");
            Directory.CreateDirectory(folder);

            File.WriteAllText(Path.Combine(folder, "a.sav"), "A");
            File.WriteAllText(Path.Combine(folder, "b.sav"), "B");

            var files = SaveData.GetFiles(folder, "*.sav");

            Assert.AreEqual(2, files.Length);

            Directory.Delete(folder, true);
        }
    }
}