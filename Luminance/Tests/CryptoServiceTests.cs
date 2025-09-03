using System.Text;
using Luminance.Helpers;
using Luminance.Services;

namespace Luminance.Tests
{
    public static class CryptoServiceTests
    {
        public static ICryptoService cryptoService = new CryptoService();
        public static void RunAll()
        {
            RunGeneratePasswordSaltTests();
            RunGenerateUserKeyTests();
            RunGenerateRecoveryKeyTests();
            RunGenerateFieldKeyTests();
            RunHashUserNameTests();
            Run_EncryptToBase64_DecryptFromBase64_Tests();
            Run_EncryptData_DecryptData_Tests();
        }

        public static void RunGeneratePasswordSaltTests()
        {
            GeneratePasswordSalt_Returns16ByteValue();
            GeneratePasswordSalt_ReturnsUniqueValues();
            GeneratePasswordSalt_ReturnsValidBase64();
        }

        public static void RunGenerateUserKeyTests ()
        {
            GenerateUserKey_SameInput_SameOutput();
            GenerateUserKey_DifferentInput_DifferentOutput();
            GenerateUserKey_DifferentSalt_DifferentOutput();
            GenerateUserKey_ReturnsCorrectLengthBase64();
            GenerateUserKey_ReturnsExpectedHash();
        }

        public static void RunGenerateRecoveryKeyTests()
        {
            GenerateRecoveryKey_ReturnsUniqueValues();
            GenerateRecoveryKey_Returns32ByteValue();
        }

        public static void RunGenerateFieldKeyTests()
        {
            GenerateFieldKey_ReturnsUniqueValues();
            GenerateFieldKey_Returns16ByteValue();
            GenerateFieldKey_ReturnsValidBase64();
        }

        public static void RunHashUserNameTests()
        {
            HashUserName_SameInput_SameOutput();
            HashUserName_DifferentInput_DifferentOutput();
        }

        private static void Run_EncryptToBase64_DecryptFromBase64_Tests()
        {
            EncryptToBase64_Then_DecryptFromBase64_ReturnsOriginal();
            EncryptToBase64_SameInput_SameKey_DifferentCiphertext();
            DecryptFromBase64_MultipleEncrypted_StillReturnsOriginal();
            EncryptToBase64_DifferentInputs_DifferentCiphertext();
        }

        public static void Run_EncryptData_DecryptData_Tests()
        {
            EncryptData_Then_DecryptData_ReturnsOriginal();
            EncryptData_SameInput_SameKey_DifferentCiphertext();
            DecryptData_MultipleEncrypted_StillReturnsOriginal();
            EncryptData_DifferentInputs_DifferentCiphertext();
        }

        //Password salt generation tests.
        private static void GeneratePasswordSalt_Returns16ByteValue()
        {
            string saltBase64 = cryptoService.GeneratePasswordSalt();
            var saltBytes = Convert.FromBase64String(saltBase64);
            string message = "GeneratePasswordSalt result should be 16 bytes.";

            Assert.Equal(16, saltBytes.Length, message);
        }

        private static void GeneratePasswordSalt_ReturnsUniqueValues()
        {
            string salt_one = cryptoService.GeneratePasswordSalt();
            string salt_two = cryptoService.GeneratePasswordSalt();
            string message = "GeneratePasswordSalt result should always be unique.";

            Assert.NotEqual(salt_one, salt_two, message);
        }

        private static void GeneratePasswordSalt_ReturnsValidBase64()
        {
            string message = "GeneratePasswordSalt result should be valid Base64 string";

            Assert.DoesNotThrow(() =>
            {
                var salt = cryptoService.GeneratePasswordSalt();
                var bytes = Convert.FromBase64String(salt);
            }, message);
        }

        //UserKey generation tests.

        private static (string password, string passwordSalt) GetPasswordAndSalt()
        {
            string password = "test1234";
            string passwordSalt = cryptoService.GeneratePasswordSalt();

            return (password, passwordSalt);
        }

        private static void  GenerateUserKey_SameInput_SameOutput()
        {
            var inputValues = GetPasswordAndSalt();
            string message = "GenerateUserKey should return same output on same input.";

            string userKey_one = cryptoService.GenerateUserKey(inputValues.password, inputValues.passwordSalt);
            string userKey_two = cryptoService.GenerateUserKey(inputValues.password, inputValues.passwordSalt);

            Assert.Equal(userKey_one, userKey_two, message);
        }

        private static void GenerateUserKey_DifferentInput_DifferentOutput()
        {
            var inputValues = GetPasswordAndSalt();
            string alternatePassword = "test1337";
            string message = "GenerateUserKey should return different output on different input.";

            string userKey_one = cryptoService.GenerateUserKey(inputValues.password, inputValues.passwordSalt);
            string userKey_two = cryptoService.GenerateUserKey(alternatePassword, inputValues.passwordSalt);

            Assert.NotEqual(userKey_one, userKey_two, message);
        }

        private static void GenerateUserKey_DifferentSalt_DifferentOutput()
        {
            var inputValues = GetPasswordAndSalt();
            string alternateSalt = cryptoService.GeneratePasswordSalt();
            string message = "GenerateUserKey should return different output with different salt.";

            string userKey_one = cryptoService.GenerateUserKey(inputValues.password, inputValues.passwordSalt);
            string userKey_two = cryptoService.GenerateUserKey(inputValues.password, alternateSalt);

            Assert.NotEqual(userKey_one, userKey_two, message);
        }

        private static void GenerateUserKey_ReturnsCorrectLengthBase64()
        {
            var inputValues = GetPasswordAndSalt();
            string message = "GenerateUserKey should return correct length base64 string.";

            var key = cryptoService.GenerateUserKey(inputValues.password, inputValues.passwordSalt);

            Assert.Equal(44, key.Length, message);
        }

        private static void GenerateUserKey_ReturnsExpectedHash()
        {
            string password = "test1234";
            string salt = "TO4o3NxfHI8dTMjX7u5WPw==";
            string expectedResult = "HIa64DyVXGc8uYu7U0/F9LaO/zFrnpTFI33cXct4VC4=";
            string message = "GenerateUserKey should return expected value.";

            var key = cryptoService.GenerateUserKey(password, salt);

            Assert.Equal(key, expectedResult, message);
        }


        //RecoveryKey generation tests.

        private static void GenerateRecoveryKey_ReturnsUniqueValues()
        {
            string key_one = cryptoService.GenerateRecoveryKey();
            string key_two = cryptoService.GenerateRecoveryKey();
            string message = "GenerateRecoveryKey result should always be unique.";

            Assert.NotEqual(key_one, key_two, message);
        }

        private static void GenerateRecoveryKey_Returns32ByteValue()
        {
            string recoveryKey = cryptoService.GenerateRecoveryKey();
            string message = "GenerateRecoveryKey result should be 32 bytes.";

            Assert.Equal(32, recoveryKey.Length, message);
        }


        //FieldKey generation tests.

        private static void GenerateFieldKey_ReturnsUniqueValues()
        {
            string key_one = cryptoService.GenerateFieldKey();
            string key_two = cryptoService.GenerateFieldKey();
            string message = "GenerateFieldKey result should always be unique.";

            Assert.NotEqual(key_one, key_two, message);
        }

        private static void GenerateFieldKey_Returns16ByteValue()
        {
            string RecoveryKeyBase64 = cryptoService.GenerateFieldKey();
            var recoveryKeyBytes = Convert.FromBase64String(RecoveryKeyBase64);
            string message = "GenerateFieldKey result should be 16 bytes.";

            Assert.Equal(16, recoveryKeyBytes.Length, message);
        }

        private static void GenerateFieldKey_ReturnsValidBase64()
        {
            string message = "GenerateFieldKey result should be valid Base64 string";

            Assert.DoesNotThrow(() =>
            {
                var key = cryptoService.GenerateFieldKey();
                var bytes = Convert.FromBase64String(key);
            }, message);
        }

        //UserName hashing Tests

        private static void HashUserName_SameInput_SameOutput()
        {
            string name_one = cryptoService.HashUserName("test1234");
            string name_two = cryptoService.HashUserName("test1234");
            string message = "HashUserName should return the same value on same input value.";

            Assert.Equal(name_one, name_two, message);
        }

        private static void HashUserName_DifferentInput_DifferentOutput()
        {
            string name_one = cryptoService.HashUserName("test1234");
            string name_two = cryptoService.HashUserName("test4321");
            string message = "HashUserName should return different value on different input value.";

            Assert.NotEqual(name_one, name_two, message);
        }

        //EncryptToBase64 and DecryptFromBase64 tests

        private static void EncryptToBase64_Then_DecryptFromBase64_ReturnsOriginal()
        {
            string input = "encryption test string";
            var inputBytes = Encoding.UTF8.GetBytes(input);
            string fieldKey = cryptoService.GenerateFieldKey();
            var fieldKeyBytes = CryptoService.ConvertTo16ByteKey(fieldKey);
            string message = "EncryptToBase64 should return the original string after encryption";

            string encrypted = CryptoService.EncryptToBase64(inputBytes, fieldKeyBytes);
            var decryptedBytes = CryptoService.DecryptFromBase64(encrypted, fieldKeyBytes);
            string decrypted = Encoding.UTF8.GetString(decryptedBytes);

            Assert.Equal(input, decrypted, message);
        }

        private static void EncryptToBase64_SameInput_SameKey_DifferentCiphertext()
        {
            string input = "encryption test string";
            var inputBytes = Encoding.UTF8.GetBytes(input);
            string fieldKey = cryptoService.GenerateFieldKey();
            var fieldKeyBytes = CryptoService.ConvertTo16ByteKey(fieldKey);
            string message = "EncryptToBase64 should return different ciphertexts due to random nonce";

            string encrypted_one = CryptoService.EncryptToBase64(inputBytes, fieldKeyBytes);
            string encrypted_two = CryptoService.EncryptToBase64(inputBytes, fieldKeyBytes);

            Assert.NotEqual(encrypted_one, encrypted_two, message);
        }

        private static void DecryptFromBase64_MultipleEncrypted_StillReturnsOriginal()
        {
            string input = "encryption test string";
            var inputBytes = Encoding.UTF8.GetBytes(input);
            string fieldKey = cryptoService.GenerateFieldKey();
            var fieldKeyBytes = CryptoService.ConvertTo16ByteKey(fieldKey);
            string message_one = "DecryptFromBase64 value #1 should match original input";
            string message_two = "DecryptFromBase64 value #2 should match original input";

            string encrypted_one = CryptoService.EncryptToBase64(inputBytes, fieldKeyBytes);
            string encrypted_two = CryptoService.EncryptToBase64(inputBytes, fieldKeyBytes);

            var decryptedBytes_one = CryptoService.DecryptFromBase64(encrypted_one, fieldKeyBytes);
            var decryptedBytes_two = CryptoService.DecryptFromBase64(encrypted_two, fieldKeyBytes);

            string decrypted_one= Encoding.UTF8.GetString(decryptedBytes_one);
            string decrypted_two = Encoding.UTF8.GetString(decryptedBytes_two);

            Assert.Equal(input, decrypted_one, message_one);
            Assert.Equal(input, decrypted_two, message_two);
        }

        private static void EncryptToBase64_DifferentInputs_DifferentCiphertext()
        {
            string input_one = "encryption test string";
            string input_two = "encryption test string two";
            var inputBytes_one = Encoding.UTF8.GetBytes(input_one);
            var inputBytes_two = Encoding.UTF8.GetBytes(input_two);
            string fieldKey = cryptoService.GenerateFieldKey();
            var fieldKeyBytes = CryptoService.ConvertTo16ByteKey(fieldKey);
            string message = "Encrypting different inputs should result in different outputs";

            string encrypted_one = CryptoService.EncryptToBase64(inputBytes_one, fieldKeyBytes);
            string encrypted_two = CryptoService.EncryptToBase64(inputBytes_two, fieldKeyBytes);

            Assert.NotEqual(encrypted_one, encrypted_two, message);
        }

        //EncryptData and DecryptData tests

        private static void EncryptData_Then_DecryptData_ReturnsOriginal()
        {
            string input = "encryption test string";
            string fieldKey = cryptoService.GenerateFieldKey();
            string message = "DecryptData should return the original string after encryption";

            string encrypted = cryptoService.EncryptData(input, fieldKey);
            string decrypted = cryptoService.DecryptData(encrypted, fieldKey);

            Assert.Equal(input, decrypted, message);
        }

        private static void EncryptData_SameInput_SameKey_DifferentCiphertext()
        {
            string input = "encryption test string";
            string fieldKey = cryptoService.GenerateFieldKey();
            string message = "EncryptData should return different ciphertexts due to random nonce";

            string encrypted_one = cryptoService.EncryptData(input, fieldKey);
            string encrypted_two = cryptoService.EncryptData(input, fieldKey);

            Assert.NotEqual(encrypted_one, encrypted_two, message);
        }

        private static void DecryptData_MultipleEncrypted_StillReturnsOriginal()
        {
            string input = "encryption test string";
            string fieldKey = cryptoService.GenerateFieldKey();
            string message_one = "DecryptData value #1 should match original input";
            string message_two = "DecryptData value #2 should match original input";

            string encrypted_one = cryptoService.EncryptData(input, fieldKey);
            string encrypted_two = cryptoService.EncryptData(input, fieldKey);

            string decrypted_one = cryptoService.DecryptData(encrypted_one, fieldKey);
            string decrypted_two = cryptoService.DecryptData(encrypted_two, fieldKey);

            Assert.Equal(input, decrypted_one, message_one);
            Assert.Equal(input, decrypted_two, message_two);
        }

        private static void EncryptData_DifferentInputs_DifferentCiphertext()
        {
            string input_one = "string one";
            string input_two = "string two";
            string fieldKey = cryptoService.GenerateFieldKey();
            string message = "Encrypting different inputs should result in different outputs";

            string encrypted_one = cryptoService.EncryptData(input_one, fieldKey);
            string encrypted_two = cryptoService.EncryptData(input_two, fieldKey);

            Assert.NotEqual(encrypted_one, encrypted_two, message);
        }

    }
}
