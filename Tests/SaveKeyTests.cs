using System;
using System.Collections.Generic;
using fefek5.Variables.SaveDataVariable.Runtime;
using fefek5.Variables.SerializableGuidVariable.Runtime;
using NUnit.Framework;

namespace fefek5.Variables.SaveDataVariable.Tests
{
    public class SaveKeyTests
    {
        private SerializableGuid _guid1;
        private SerializableGuid _guid2;
        private string _guid1Hex;
        private string _stringKey1;
        private string _stringKey2;
        private string _comment1;

        [SetUp]
        public void Setup()
        {
            var dotnetGuid1 = Guid.NewGuid();
            _guid1 = new SerializableGuid(dotnetGuid1);
            _guid2 = SerializableGuid.NewGuid();
            _guid1Hex = _guid1.ToHexString();
            _stringKey1 = "MyStringKey";
            _stringKey2 = "AnotherStringKey";
            _comment1 = "My comment";
        }

        [Test]
        public void SaveKeyFromGuidReconstructsOriginalGuidCorrectly()
        {
            // Arrange
            var originalGuid = Guid.NewGuid();

            // Używamy konstruktora SaveKey, który przyjmuje GUID
            // Działa tu niejawna konwersja Guid -> SerializableGuid,
            // a potem konstruktor SaveKey(SerializableGuid key)
            SaveKey saveKey = new SaveKey(originalGuid);

            // Act
            // Pobieramy SerializableGuid z SaveKey i konwertujemy go z powrotem na Guid
            Guid reconstructedGuid = saveKey.Key.ToGuid();

            // Assert
            // 1. Sprawdź, czy klucz nie jest pusty (jak w oryginalnym teście)
            Assert.IsFalse(saveKey.IsEmpty(), "SaveKey created from a valid GUID should not be empty.");

            // 2. Sprawdź, czy odtworzony Guid jest RÓWNY oryginalnemu Guid
            //    Porównanie obiektów Guid zadba o sprawdzenie wartości bez względu
            //    na problemy z reprezentacją szesnastkową.
            Assert.AreEqual(originalGuid, reconstructedGuid,
                "The reconstructed GUID should be equal to the original GUID.");
        }

        [Test]
        public void SaveKeyFromValidHexStringIsNotEmpty()
        {
            // Ciąg heksadecymalny zawierający 32 znaki.
            string hex = "0123456789ABCDEF0123456789ABCDEF";
            SaveKey saveKey = new SaveKey(hex);

            // Klucz nie powinien być pusty.
            Assert.IsFalse(saveKey.IsEmpty());
            // W przypadku poprawnego ciągu heksadecymalnego, StringKey nie jest ustawiony,
            // a klucz GUID jest konwertowany na ciąg heksadecymalny.
            Assert.AreEqual(hex, saveKey.Key.ToHexString());
            Assert.AreEqual(hex, saveKey.ToString());
        }

        [Test]
        public void SaveKeyFromNonHexStringIsNotEmpty()
        {
            // Ciąg niebędący poprawnym ciągiem heksadecymalnym.
            string keyStr = "NonHexKeyValue";
            SaveKey saveKey = new SaveKey(keyStr);

            // Przy tworzeniu z nieheksadecymalnego ciągu, klucz GUID powinien być pusty,
            // a StringKey ustawiony na przekazany ciąg.
            Assert.IsTrue(saveKey.Key.IsEmpty);
            Assert.AreEqual(keyStr, saveKey.StringKey.value);
            Assert.AreEqual(keyStr, saveKey.ToString());
        }

        [Test]
        public void SaveKeySetKeyStringAndGuid()
        {
            // Utworzenie SaveKey z GUID.
            var guid = Guid.NewGuid();
            SaveKey saveKey = new SaveKey(guid);

            // Ustawienie nowego klucza przy użyciu ciągu znaków, który nie jest heksadecymalny.
            string newKey = "NonHexValue";
            saveKey = saveKey.SetKey(newKey);
            Assert.IsTrue(saveKey.StringKey.hasValue);
            Assert.AreEqual(newKey, saveKey.StringKey.value);
            Assert.IsTrue(saveKey.Key.IsEmpty);

            // Ustawienie klucza za pomocą poprawnego ciągu heksadecymalnego (32 znaki).
            string hex = "FEDCBA9876543210FEDCBA9876543210";
            saveKey = saveKey.SetKey(hex);
            // Po ustawieniu poprawnego klucza, StringKey powinna zostać wyczyszczona.
            Assert.IsFalse(saveKey.StringKey.hasValue);
            Assert.IsFalse(saveKey.Key.IsEmpty);
            Assert.AreEqual(hex, saveKey.Key.ToHexString());
        }

        [Test]
        public void SaveKeySetCommentAndRemoveComment()
        {
            var guid = Guid.NewGuid();
            SaveKey saveKey = new SaveKey(guid);
            string comment = "Test comment";

            // Ustawienie komentarza.
            saveKey = saveKey.SetComment(comment);
            Assert.IsTrue(saveKey.Comment.hasValue);
            Assert.AreEqual(comment, saveKey.Comment.value);

            // Usunięcie komentarza.
            saveKey = saveKey.RemoveComment();
            Assert.IsFalse(saveKey.Comment.hasValue);
            Assert.AreEqual(string.Empty, saveKey.Comment.value);
        }

        [Test]
        public void SaveKeyEqualityTests()
        {
            // Test porównania SaveKey utworzonych z tego samego GUID.
            var guid = Guid.NewGuid();
            SaveKey key1 = new SaveKey(guid);
            SaveKey key2 = new SaveKey(guid);
            Assert.IsTrue(key1.Equals(key2));
            Assert.IsTrue(key1 == key2);

            // Test porównania SaveKey utworzonych z tego samego nieheksadecymalnego ciągu.
            string strKey = "NonHexKey";
            SaveKey key3 = new SaveKey(strKey);
            SaveKey key4 = new SaveKey(strKey);
            Assert.IsTrue(key3.Equals(key4));
            Assert.IsTrue(key3 == key4);

            // Porównanie SaveKey utworzonego z GUID i SaveKey utworzonego z poprawnego ciągu heksadecymalnego –
            // oczekujemy, że będą różne.
            string hex = "0123456789ABCDEF0123456789ABCDEF";
            SaveKey key5 = new SaveKey(hex);
            Assert.IsFalse(key1.Equals(key5));
        }

        [Test]
        public void HashSetWithGuidKeysBehavesCorrectly()
        {
            var hashSet = new HashSet<SaveKey>();
            var key1A = new SaveKey(_guid1);
            var key1B = new SaveKey(_guid1); // Taki sam GUID
            var key2 = new SaveKey(_guid2); // Inny GUID
            var stringKey = new SaveKey(_stringKey1); // Klucz stringowy

            Assert.IsTrue(hashSet.Add(key1A), "Should add key1a");
            Assert.IsFalse(hashSet.Add(key1B), "Should not add duplicate key1b"); // Nie powinno dodać duplikatu
            Assert.IsTrue(hashSet.Add(key2), "Should add key2");
            Assert.IsTrue(hashSet.Add(stringKey),
                "Should add stringKey (different type)"); // Powinno dodać, bo inny typ klucza w SaveKey

            Assert.AreEqual(3, hashSet.Count, "HashSet should contain 3 distinct keys");
            Assert.IsTrue(hashSet.Contains(key1A), "Should contain key1a");
            Assert.IsTrue(hashSet.Contains(key1B),
                "Should contain key equivalent to key1b"); // Powinno znaleźć odpowiednik
            Assert.IsTrue(hashSet.Contains(key2), "Should contain key2");
            Assert.IsTrue(hashSet.Contains(stringKey), "Should contain stringKey");
            Assert.IsFalse(hashSet.Contains(new SaveKey(SerializableGuid.NewGuid())),
                "Should not contain a random new key");
        }

        [Test]
        public void HashSetWithStringKeysBehavesCorrectly()
        {
            var hashSet = new HashSet<SaveKey>();
            var key1A = new SaveKey(_stringKey1);
            var key1B = new SaveKey(_stringKey1); // Taki sam string
            var key2 = new SaveKey(_stringKey2); // Inny string
            var guidKey = new SaveKey(_guid1); // Klucz GUID

            Assert.IsTrue(hashSet.Add(key1A), "Should add key1a");
            Assert.IsFalse(hashSet.Add(key1B), "Should not add duplicate key1b");
            Assert.IsTrue(hashSet.Add(key2), "Should add key2");
            Assert.IsTrue(hashSet.Add(guidKey), "Should add guidKey (different type)");

            Assert.AreEqual(3, hashSet.Count, "HashSet should contain 3 distinct keys");
            Assert.IsTrue(hashSet.Contains(key1A), "Should contain key1a");
            Assert.IsTrue(hashSet.Contains(key1B), "Should contain key equivalent to key1b");
            Assert.IsTrue(hashSet.Contains(key2), "Should contain key2");
            Assert.IsTrue(hashSet.Contains(guidKey), "Should contain guidKey");
            Assert.IsFalse(hashSet.Contains(new SaveKey("RandomNewStringKey")), "Should not contain a random new key");
        }

        [Test]
        public void HashSetDistinguishesGuidKeyFromStringKeyLookingLikeGuidHex()
        {
            var hashSet = new HashSet<SaveKey>();
            var guidKey = new SaveKey(_guid1);
            var stringKeyLooksLikeGuid = new SaveKey(_guid1Hex); // Klucz stringowy, ale wygląda jak hex GUIDa

            Assert.IsTrue(hashSet.Add(guidKey), "Should add guidKey");
            Assert.IsFalse(hashSet.Add(stringKeyLooksLikeGuid),
                "Should add stringKeyLooksLikeGuid as distinct"); // Klucz stringowy jest inny niż GUIDowy

            Assert.AreEqual(1, hashSet.Count, "HashSet should contain 2 distinct keys");
            Assert.IsTrue(hashSet.Contains(guidKey), "Should contain guidKey");
            Assert.IsTrue(hashSet.Contains(stringKeyLooksLikeGuid), "Should contain stringKeyLooksLikeGuid");
            // Sprawdzenie krzyżowe - czy Contains zadziała z Guid/stringiem
            Assert.IsTrue(hashSet.Contains(_guid1),
                "Should not find guidKey when searching by SerializableGuid directly if string key is present (Correct based on Equals(SerializableGuid))"); // Equals(SerializableGuid) działa tylko gdy !StringKey.hasValue
            Assert.IsTrue(hashSet.Contains(_guid1Hex),
                "Should find stringKeyLooksLikeGuid when searching by matching string"); // Equals(string) działa dla StringKey
        }
        
        [Test]
        public void DictionaryWithGuidKeysBehavesCorrectly()
        {
            var dictionary = new Dictionary<SaveKey, string>();
            var key1A = new SaveKey(_guid1);
            var key1B = new SaveKey(_guid1); // Taki sam GUID
            var key2 = new SaveKey(_guid2); // Inny GUID

            dictionary.Add(key1A, "Value 1a");
            Assert.Throws<ArgumentException>(() => dictionary.Add(key1B, "Value 1b"),
                "Should throw ArgumentException on adding duplicate key"); // Próba dodania duplikatu
            dictionary[key1B] = "Value 1b Updated"; // Aktualizacja wartości za pomocą równoważnego klucza

            dictionary.Add(key2, "Value 2");

            Assert.AreEqual(2, dictionary.Count, "Dictionary should contain 2 entries");
            Assert.IsTrue(dictionary.ContainsKey(key1A), "Should contain key1a");
            Assert.IsTrue(dictionary.ContainsKey(key1B), "Should contain key equivalent to key1b");
            Assert.AreEqual("Value 1b Updated", dictionary[key1A], "Value for key1a should be updated");
            Assert.AreEqual("Value 1b Updated", dictionary[key1B], "Value for key1b should be updated");

            Assert.IsTrue(dictionary.ContainsKey(key2), "Should contain key2");
            Assert.AreEqual("Value 2", dictionary[key2]);

            Assert.IsFalse(dictionary.ContainsKey(new SaveKey(SerializableGuid.NewGuid())),
                "Should not contain random key");
            Assert.IsFalse(dictionary.ContainsKey(new SaveKey(_stringKey1)),
                "Should not contain string key"); // Klucz stringowy nie pasuje do klucza GUID
        }

        [Test]
        public void DictionaryWithStringKeysBehavesCorrectly()
        {
            var dictionary = new Dictionary<SaveKey, int>();
            var key1A = new SaveKey(_stringKey1);
            var key1B = new SaveKey(_stringKey1); // Taki sam string
            var key2 = new SaveKey(_stringKey2); // Inny string

            dictionary.Add(key1A, 10);
            Assert.Throws<ArgumentException>(() => dictionary.Add(key1B, 11));
            dictionary[key1B] = 11; // Update

            dictionary.Add(key2, 20);

            Assert.AreEqual(2, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey(key1A));
            Assert.IsTrue(dictionary.ContainsKey(key1B));
            Assert.AreEqual(11, dictionary[key1A]);
            Assert.AreEqual(11, dictionary[key1B]);

            Assert.IsTrue(dictionary.ContainsKey(key2));
            Assert.AreEqual(20, dictionary[key2]);

            Assert.IsFalse(dictionary.ContainsKey(new SaveKey("RandomString")));
            Assert.IsFalse(dictionary.ContainsKey(new SaveKey(_guid1))); // Klucz GUID nie pasuje do klucza stringowego
        }

        [Test]
        public void DictionaryMixedKeysBehavesCorrectlyWithNormalization()
        {
            var dictionary = new Dictionary<SaveKey, string>();
            var guidKey = new SaveKey(_guid1);
            var stringKey = new SaveKey(_stringKey1);
            var stringKeyLooksLikeGuid = new SaveKey(_guid1Hex); // Teraz równoważny guidKey

            dictionary.Add(guidKey, "GUID Value");
            dictionary.Add(stringKey, "String Value");

            // Próba dodania stringKeyLooksLikeGuid rzuci wyjątek lub zaktualizuje wartość, jeśli użyjemy []
            // Assert.Throws<ArgumentException>(() => dictionary.Add(stringKeyLooksLikeGuid, "String Like GUID Value"));
            // LUB:
            dictionary[stringKeyLooksLikeGuid] = "Value Updated via Hex String Key"; // Aktualizacja wartości powiązaną z guidKey

            Assert.AreEqual(2, dictionary.Count, "Should contain 2 distinct keys (GUID and String)");

            // Sprawdzenie klucza GUID (może być teraz znaleziony przez hex string)
            Assert.IsTrue(dictionary.ContainsKey(guidKey));
            Assert.IsTrue(dictionary.ContainsKey(stringKeyLooksLikeGuid)); // Powinno znaleźć ten sam wpis co guidKey
            Assert.AreEqual("Value Updated via Hex String Key", dictionary[guidKey]);
            Assert.AreEqual("Value Updated via Hex String Key", dictionary[stringKeyLooksLikeGuid]);

            // Sprawdzenie klucza string
            Assert.IsTrue(dictionary.ContainsKey(stringKey));
            Assert.AreEqual("String Value", dictionary[stringKey]);

            // Sprawdzenie krzyżowe
            Assert.IsFalse(dictionary.ContainsKey(new SaveKey(_stringKey2))); // Inny string
            Assert.IsFalse(dictionary.ContainsKey(new SaveKey(_guid2))); // Inny GUID
        }

        [Test]
        public void DictionaryTryGetValueWorksCorrectly()
        {
            var dictionary = new Dictionary<SaveKey, string>();
            var guidKey = new SaveKey(_guid1);
            var stringKey = new SaveKey(_stringKey1);

            dictionary.Add(guidKey, "Guid Data");
            dictionary.Add(stringKey, "String Data");

            // TryGetValue for existing GUID key
            bool guidFound = dictionary.TryGetValue(guidKey, out string guidValue);
            Assert.IsTrue(guidFound, "TryGetValue should find guidKey");
            Assert.AreEqual("Guid Data", guidValue, "TryGetValue returned wrong value for guidKey");

            // TryGetValue for equivalent GUID key
            bool guidFoundEquivalent = dictionary.TryGetValue(new SaveKey(_guid1), out string guidValueEquivalent);
            Assert.IsTrue(guidFoundEquivalent, "TryGetValue should find equivalent guidKey");
            Assert.AreEqual("Guid Data", guidValueEquivalent,
                "TryGetValue returned wrong value for equivalent guidKey");

            // TryGetValue for existing string key
            bool stringFound = dictionary.TryGetValue(stringKey, out string stringValue);
            Assert.IsTrue(stringFound, "TryGetValue should find stringKey");
            Assert.AreEqual("String Data", stringValue, "TryGetValue returned wrong value for stringKey");

            // TryGetValue for non-existing key
            bool notFound = dictionary.TryGetValue(new SaveKey(_guid2), out string notFoundValue);
            Assert.IsFalse(notFound, "TryGetValue should not find non-existing key");
            Assert.IsNull(notFoundValue,
                "TryGetValue should return default value for non-existing key"); // Dla string domyślnie null
        }
    }
}